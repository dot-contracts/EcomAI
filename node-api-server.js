const express = require('express');
const cors = require('cors');
const { Pool } = require('pg');
const { v4: uuidv4 } = require('uuid');

const app = express();
const port = 5000;

// CORS configuration for React Native
app.use(cors({
  origin: true,
  credentials: true,
  methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
  allowedHeaders: ['Content-Type', 'Authorization', 'Accept']
}));

app.use(express.json());

// PostgreSQL connection
const pool = new Pool({
  user: 'postgres',
  host: 'localhost',
  database: 'ecomvideoai_db',
  password: 'EcomVideoAI2024!',
  port: 5432,
});

// Test database connection
pool.connect()
  .then(() => console.log('✅ Connected to PostgreSQL database'))
  .catch(err => console.error('❌ Database connection error:', err));

// Health check endpoint
app.get('/health', (req, res) => {
  res.json('Healthy');
});

// Test database connection endpoint
app.get('/api/Video/test-db', async (req, res) => {
  try {
    const result = await pool.query('SELECT COUNT(*) FROM videos');
    res.json({ 
      message: 'Database connection successful', 
      videoCount: parseInt(result.rows[0].count),
      timestamp: new Date().toISOString()
    });
  } catch (error) {
    console.error('Database test error:', error);
    res.status(500).json({ error: 'Database connection failed', details: error.message });
  }
});

// Create video from text endpoint
app.post('/api/Video/create-from-text', async (req, res) => {
  try {
    console.log('📥 Received video creation request:', req.body);
    
    const {
      userId,
      textPrompt,
      title,
      description,
      resolution,
      style,
      duration,
      aspectRatio
    } = req.body;

    // Validate required fields
    if (!userId || !textPrompt || !title) {
      return res.status(400).json({
        error: 'Missing required fields: userId, textPrompt, title'
      });
    }

    // Check if user exists
    const userCheck = await pool.query('SELECT id FROM users WHERE id = $1', [userId]);
    if (userCheck.rows.length === 0) {
      return res.status(400).json({
        error: 'User not found',
        userId: userId
      });
    }

    // Convert aspect ratio string to enum value
    const aspectRatioMap = {
      '9:16': 0,  // Portrait916
      '16:9': 1,  // Landscape169
      '1:1': 2,   // Square11
      '4:5': 3,   // Portrait45
      '2:3': 4,   // Portrait23
    };

    const aspectRatioValue = aspectRatioMap[aspectRatio || '9:16'] ?? 0;

    const videoId = uuidv4();
    const createdAt = new Date().toISOString();

    // Insert video into database
    const insertQuery = `
      INSERT INTO videos (
        id, user_id, title, description, text_prompt, input_type, 
        status, resolution, duration_seconds, aspect_ratio, style,
        created_at, file_size_bytes
      ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13)
      RETURNING *
    `;

    const values = [
      videoId,
      userId,
      title,
      description || '',
      textPrompt,
      'text',
      'Pending',
      parseInt(resolution) || 1,
      duration || 3,
      aspectRatioValue,
      style || 'realistic',
      createdAt,
      0
    ];

    const result = await pool.query(insertQuery, values);
    const video = result.rows[0];

    console.log('✅ Video created successfully:', videoId);

    // Map the enum value back to string for response
    const aspectRatioDisplayMap = {
      0: '9:16',
      1: '16:9',
      2: '1:1',
      3: '4:5',
      4: '2:3'
    };

    // Return response in expected format
    res.status(201).json({
      id: video.id,
      userId: video.user_id,
      title: video.title,
      description: video.description,
      textPrompt: video.text_prompt,
      inputType: video.input_type,
      status: video.status,
      resolution: video.resolution,
      durationSeconds: video.duration_seconds,
      aspectRatio: aspectRatioDisplayMap[video.aspect_ratio] || '9:16',
      style: video.style,
      imageUrl: video.image_url,
      videoUrl: video.video_url,
      thumbnailUrl: video.thumbnail_url,
      completedAt: video.completed_at,
      errorMessage: video.error_message,
      fileSizeBytes: video.file_size_bytes,
      metadata: video.metadata,
      createdAt: video.created_at,
      updatedAt: video.updated_at,
      message: 'Video creation started successfully'
    });

  } catch (error) {
    console.error('❌ Error creating video:', error);
    res.status(500).json({
      error: 'Failed to create video',
      details: error.message
    });
  }
});

// Get video by ID endpoint
app.get('/api/Video/:videoId', async (req, res) => {
  try {
    const { videoId } = req.params;
    console.log('📥 Getting video:', videoId);

    const result = await pool.query('SELECT * FROM videos WHERE id = $1', [videoId]);
    
    if (result.rows.length === 0) {
      return res.status(404).json({
        error: 'Video not found',
        videoId: videoId
      });
    }

    const video = result.rows[0];
    
    // Return response in expected format
    res.json({
      id: video.id,
      userId: video.user_id,
      title: video.title,
      description: video.description,
      textPrompt: video.text_prompt,
      inputType: video.input_type,
      status: video.status,
      resolution: video.resolution,
      durationSeconds: video.duration_seconds,
      aspectRatio: video.aspect_ratio,
      style: video.style,
      imageUrl: video.image_url,
      videoUrl: video.video_url,
      thumbnailUrl: video.thumbnail_url,
      completedAt: video.completed_at,
      errorMessage: video.error_message,
      fileSizeBytes: video.file_size_bytes,
      metadata: video.metadata,
      createdAt: video.created_at,
      updatedAt: video.updated_at
    });

  } catch (error) {
    console.error('❌ Error getting video:', error);
    res.status(500).json({
      error: 'Failed to get video',
      details: error.message
    });
  }
});

// Get video status endpoint
app.get('/api/Video/:videoId/status', async (req, res) => {
  try {
    const { videoId } = req.params;
    console.log('📥 Getting video status:', videoId);

    const result = await pool.query('SELECT id, status, created_at FROM videos WHERE id = $1', [videoId]);
    
    if (result.rows.length === 0) {
      return res.status(404).json({
        error: 'Video not found',
        videoId: videoId
      });
    }

    const video = result.rows[0];
    
    // Calculate progress based on status and time elapsed
    let progress = 0;
    const createdAt = new Date(video.created_at);
    const now = new Date();
    const elapsedMinutes = (now - createdAt) / (1000 * 60);
    
    switch (video.status) {
      case 'Pending':
        progress = Math.min(15 + (elapsedMinutes * 2), 25);
        break;
      case 'GeneratingImage':
        progress = Math.min(30 + (elapsedMinutes * 3), 45);
        break;
      case 'GeneratingVideo':
        progress = Math.min(60 + (elapsedMinutes * 4), 85);
        break;
      case 'Processing':
        progress = Math.min(80 + (elapsedMinutes * 2), 95);
        break;
      case 'Completed':
        progress = 100;
        break;
      case 'Failed':
        progress = 0;
        break;
      default:
        progress = 20;
    }

    // Simulate video processing progression
    if (video.status === 'Pending' && elapsedMinutes > 0.5) {
      // After 30 seconds, move to GeneratingImage
      await pool.query('UPDATE videos SET status = $1 WHERE id = $2', ['GeneratingImage', videoId]);
      video.status = 'GeneratingImage';
    } else if (video.status === 'GeneratingImage' && elapsedMinutes > 1.5) {
      // After 1.5 minutes, move to GeneratingVideo
      await pool.query('UPDATE videos SET status = $1 WHERE id = $2', ['GeneratingVideo', videoId]);
      video.status = 'GeneratingVideo';
    } else if (video.status === 'GeneratingVideo' && elapsedMinutes > 3) {
      // After 3 minutes, move to Processing
      await pool.query('UPDATE videos SET status = $1 WHERE id = $2', ['Processing', videoId]);
      video.status = 'Processing';
    } else if (video.status === 'Processing' && elapsedMinutes > 4) {
      // After 4 minutes, complete the video
      const videoUrl = `https://test-videos.example.com/video_${videoId}.mp4`;
      const thumbnailUrl = `https://test-videos.example.com/thumb_${videoId}.jpg`;
      
      await pool.query(
        'UPDATE videos SET status = $1, video_url = $2, thumbnail_url = $3, completed_at = $4 WHERE id = $5',
        ['Completed', videoUrl, thumbnailUrl, new Date().toISOString(), videoId]
      );
      video.status = 'Completed';
      progress = 100;
    }

    const getStatusMessage = (status) => {
      switch (status) {
        case 'Pending':
          return 'Preparing your video...';
        case 'GeneratingImage':
          return 'Creating image from your prompt...';
        case 'GeneratingVideo':
          return 'Generating video from image...';
        case 'Processing':
          return 'Finalizing your video...';
        case 'Completed':
          return 'Video generation completed!';
        case 'Failed':
          return 'Video generation failed';
        default:
          return 'Processing your request...';
      }
    };

    // Return status response
    res.json({
      id: video.id,
      status: video.status,
      progress: Math.round(progress),
      message: getStatusMessage(video.status)
    });

  } catch (error) {
    console.error('❌ Error getting video status:', error);
    res.status(500).json({
      error: 'Failed to get video status',
      details: error.message
    });
  }
});

// Get user videos endpoint
app.get('/api/Video/user/:userId', async (req, res) => {
  try {
    const { userId } = req.params;
    const { page = 1, pageSize = 10 } = req.query;
    
    console.log('📥 Getting videos for user:', userId);

    const offset = (page - 1) * pageSize;
    const result = await pool.query(
      'SELECT * FROM videos WHERE user_id = $1 ORDER BY created_at DESC LIMIT $2 OFFSET $3',
      [userId, pageSize, offset]
    );

    const videos = result.rows.map(video => ({
      id: video.id,
      userId: video.user_id,
      title: video.title,
      description: video.description,
      textPrompt: video.text_prompt,
      inputType: video.input_type,
      status: video.status,
      resolution: video.resolution,
      durationSeconds: video.duration_seconds,
      aspectRatio: video.aspect_ratio,
      style: video.style,
      imageUrl: video.image_url,
      videoUrl: video.video_url,
      thumbnailUrl: video.thumbnail_url,
      completedAt: video.completed_at,
      errorMessage: video.error_message,
      fileSizeBytes: video.file_size_bytes,
      metadata: video.metadata,
      createdAt: video.created_at,
      updatedAt: video.updated_at
    }));

    res.json(videos);

  } catch (error) {
    console.error('❌ Error getting user videos:', error);
    res.status(500).json({
      error: 'Failed to get user videos',
      details: error.message
    });
  }
});

// Delete video endpoint
app.delete('/api/Video/:videoId', async (req, res) => {
  try {
    const { videoId } = req.params;
    console.log('📥 Deleting video:', videoId);

    const result = await pool.query('DELETE FROM videos WHERE id = $1 RETURNING id', [videoId]);
    
    if (result.rows.length === 0) {
      return res.status(404).json({
        error: 'Video not found',
        videoId: videoId
      });
    }

    res.json({
      message: 'Video deleted successfully',
      videoId: videoId
    });

  } catch (error) {
    console.error('❌ Error deleting video:', error);
    res.status(500).json({
      error: 'Failed to delete video',
      details: error.message
    });
  }
});

// Start server
app.listen(port, '0.0.0.0', () => {
  console.log('🚀 EcomVideoAI Node.js API Server running on port', port);
  console.log('📡 Available endpoints:');
  console.log('  GET  /health - Health check');
  console.log('  GET  /api/Video/test-db - Database test');
  console.log('  POST /api/Video/create-from-text - Create video');
  console.log('  GET  /api/Video/:id - Get video by ID');
  console.log('  GET  /api/Video/:id/status - Get video status');
  console.log('  GET  /api/Video/user/:userId - Get user videos');
  console.log('  DELETE /api/Video/:id - Delete video');
  console.log('');
  console.log('🎯 Ready for React Native video generation!');
}); 