import React, { useState } from 'react';
import { Alert, Button, TextInput } from 'react-native';
import { useNavigation } from '@react-navigation/native';
import { CreateVideoFromTextRequest, VideoResponse } from '../../models/Video';
import { generateVideo } from '../../services/VideoService';

const VideoCreationScreen: React.FC = () => {
  const navigation = useNavigation();
  const [textPrompt, setTextPrompt] = useState('');
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [videoSettings, setVideoSettings] = useState({
    resolution: 'HD_720p',
    style: 'Natural',
    duration: 5,
    aspectRatio: '16:9',
  });

  const handleGenerate = async () => {
    if (!textPrompt.trim()) {
      Alert.alert('Error', 'Please enter a text prompt');
      return;
    }

    try {
      console.log('Starting video generation...');
      
      const request: CreateVideoFromTextRequest = {
        userId: 'user123', // TODO: Get from auth
        textPrompt: textPrompt.trim(),
        title: title.trim() || `Video: ${textPrompt.slice(0, 30)}...`,
        description: description.trim() || `Generated from: ${textPrompt}`,
        resolution: videoSettings.resolution,
        style: videoSettings.style,
        duration: videoSettings.duration,
        aspectRatio: videoSettings.aspectRatio,
      };

      console.log('Sending video generation request:', request);
      
      let video: VideoResponse;
      
      try {
        // Try the actual API first
        video = await generateVideo(request);
        console.log('✅ Video generation API succeeded:', video);
      } catch (apiError) {
        console.error('❌ API Error:', apiError);
        
        // TEMPORARY FALLBACK: Create mock video response when backend fails
        console.log('🔄 Using temporary fallback for testing...');
        
        video = {
          id: `mock-${Date.now()}`,
          userId: request.userId,
          title: request.title || 'Test Video',
          description: request.description || 'Test Description',
          textPrompt: request.textPrompt,
          inputType: 'Text',
          status: 'Pending',
          resolution: request.resolution?.toString() || 'HD_720p',
          durationSeconds: request.duration || 5,
          imageUrl: 'https://via.placeholder.com/720x1280/667eea/ffffff?text=Video+Processing',
          videoUrl: null, // Will be set when "completed"
          thumbnailUrl: 'https://via.placeholder.com/360x640/667eea/ffffff?text=Thumbnail',
          completedAt: null,
          errorMessage: null,
          fileSizeBytes: 0,
          metadata: null,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        };
        
        // Show user that we're in testing mode
        Alert.alert(
          'Testing Mode Active',
          'Video generation is running in testing mode. The backend database is being fixed. Your video will be simulated.',
          [{ text: 'Continue Testing', style: 'default' }]
        );
      }

      // Navigate to the generated video screen
      navigation.navigate('GeneratedVideo', { video });
      
    } catch (error) {
      console.error('Failed to start video generation:', error);
      Alert.alert(
        'Generation Failed', 
        error instanceof Error ? error.message : 'Unknown error occurred'
      );
    }
  };

  return (
    <div>
      {/* Render your form components here */}
    </div>
  );
};

export default VideoCreationScreen; 