# EcomVideo AI - Complete Database Schema

## 🏗️ Database Architecture Overview

This document outlines the complete database schema for the EcomVideo AI application, designed to support:
- User management and authentication
- Video generation and storage  
- Product data scraping and management
- Subscription and payment processing
- Analytics and user preferences

**Database**: PostgreSQL  
**ORM**: Entity Framework Core  
**Naming Convention**: snake_case (via UseSnakeCaseNamingConvention)

---

## 📊 Entity Relationship Diagram (Conceptual)

```
Users (1) ──→ (M) UserSubscriptions ──→ (1) SubscriptionPlans
  │
  ├── (1) ──→ (1) UserPreferences
  ├── (1) ──→ (M) PaymentMethods
  ├── (1) ──→ (M) Videos
  ├── (1) ──→ (M) ProductData
  ├── (1) ──→ (M) VideoShares
  ├── (1) ──→ (M) UserActivity
  └── (1) ──→ (M) RefreshTokens

Videos (M) ──→ (M) VideoTags (Many-to-Many)
Videos (M) ──→ (1) VideoCategories
Videos (M) ──→ (1) ProductData (optional)

ProductData (1) ──→ (M) ProductImages
ProductData (M) ──→ (1) ProductCategories

UserSubscriptions (1) ──→ (M) Payments
Payments (1) ──→ (M) Invoices
```

---

## 🔐 1. Authentication & User Management

### **users**
```sql
CREATE TABLE users (
    id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email                 VARCHAR(255) NOT NULL UNIQUE,
    email_verified        BOOLEAN DEFAULT FALSE,
    password_hash         VARCHAR(255) NOT NULL,
    first_name            VARCHAR(100),
    last_name             VARCHAR(100),
    avatar_url            VARCHAR(500),
    phone_number          VARCHAR(20),
    date_of_birth         DATE,
    timezone              VARCHAR(50) DEFAULT 'UTC',
    locale                VARCHAR(10) DEFAULT 'en-US',
    is_active             BOOLEAN DEFAULT TRUE,
    last_login_at         TIMESTAMP,
    failed_login_attempts INTEGER DEFAULT 0,
    locked_until          TIMESTAMP,
    created_at            TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at            TIMESTAMP,
    created_by            VARCHAR(100),
    updated_by            VARCHAR(100)
);

CREATE INDEX ix_users_email ON users(email);
CREATE INDEX ix_users_created_at ON users(created_at);
CREATE INDEX ix_users_is_active ON users(is_active);
```

### **user_roles**
```sql
CREATE TABLE user_roles (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_name  VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100)
);

CREATE INDEX ix_user_roles_user_id ON user_roles(user_id);
CREATE INDEX ix_user_roles_role_name ON user_roles(role_name);
```

### **refresh_tokens**
```sql
CREATE TABLE refresh_tokens (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token      VARCHAR(500) NOT NULL UNIQUE,
    expires_at TIMESTAMP NOT NULL,
    is_revoked BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    revoked_at TIMESTAMP
);

CREATE INDEX ix_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX ix_refresh_tokens_token ON refresh_tokens(token);
CREATE INDEX ix_refresh_tokens_expires_at ON refresh_tokens(expires_at);
```

### **email_verification_tokens**
```sql
CREATE TABLE email_verification_tokens (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token      VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMP NOT NULL,
    used_at    TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX ix_email_verification_tokens_user_id ON email_verification_tokens(user_id);
CREATE INDEX ix_email_verification_tokens_token ON email_verification_tokens(token);
```

### **password_reset_tokens**
```sql
CREATE TABLE password_reset_tokens (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token      VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMP NOT NULL,
    used_at    TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX ix_password_reset_tokens_user_id ON password_reset_tokens(user_id);
CREATE INDEX ix_password_reset_tokens_token ON password_reset_tokens(token);
```

---

## 🎬 2. Video Management (Enhanced)

### **video_categories**
```sql
CREATE TABLE video_categories (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    color_hex   VARCHAR(7),
    icon_name   VARCHAR(50),
    sort_order  INTEGER DEFAULT 0,
    is_active   BOOLEAN DEFAULT TRUE,
    created_at  TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMP
);

CREATE INDEX ix_video_categories_name ON video_categories(name);
CREATE INDEX ix_video_categories_is_active ON video_categories(is_active);
```

### **video_templates**
```sql
CREATE TABLE video_templates (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    category_id      UUID REFERENCES video_categories(id),
    name             VARCHAR(200) NOT NULL,
    description      TEXT,
    thumbnail_url    VARCHAR(500),
    preview_url      VARCHAR(500),
    prompt_template  TEXT NOT NULL,
    style_settings   JSONB,
    default_duration INTEGER DEFAULT 5,
    aspect_ratio     VARCHAR(10) DEFAULT '9:16',
    resolution       VARCHAR(20) DEFAULT '1080x1920',
    is_premium       BOOLEAN DEFAULT FALSE,
    is_active        BOOLEAN DEFAULT TRUE,
    usage_count      INTEGER DEFAULT 0,
    created_at       TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at       TIMESTAMP,
    created_by       VARCHAR(100),
    updated_by       VARCHAR(100)
);

CREATE INDEX ix_video_templates_category_id ON video_templates(category_id);
CREATE INDEX ix_video_templates_is_premium ON video_templates(is_premium);
CREATE INDEX ix_video_templates_is_active ON video_templates(is_active);
```

### **videos** (Enhanced version of existing)
```sql
CREATE TABLE videos (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id                 UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    category_id             UUID REFERENCES video_categories(id),
    template_id             UUID REFERENCES video_templates(id),
    product_data_id         UUID REFERENCES product_data(id),
    title                   VARCHAR(200) NOT NULL,
    description             VARCHAR(1000),
    text_prompt             VARCHAR(2000) NOT NULL,
    input_type              VARCHAR(50) NOT NULL, -- 'Text', 'Image', 'Url'
    status                  VARCHAR(50) NOT NULL, -- 'Pending', 'Processing', etc.
    resolution              VARCHAR(50) NOT NULL,
    aspect_ratio            VARCHAR(10) NOT NULL,
    duration_seconds        INTEGER NOT NULL DEFAULT 5,
    style                   VARCHAR(100),
    image_url               VARCHAR(500),
    video_url               VARCHAR(500),
    thumbnail_url           VARCHAR(500),
    freepik_task_id         VARCHAR(100),
    freepik_image_task_id   VARCHAR(100),
    completed_at            TIMESTAMP,
    error_message           VARCHAR(2000),
    file_size_bytes         BIGINT DEFAULT 0,
    view_count              INTEGER DEFAULT 0,
    download_count          INTEGER DEFAULT 0,
    share_count             INTEGER DEFAULT 0,
    is_public               BOOLEAN DEFAULT FALSE,
    is_featured             BOOLEAN DEFAULT FALSE,
    metadata                JSONB,
    ai_provider             VARCHAR(50), -- 'Freepik', 'RunwayML', 'StabilityAI', etc.
    generation_cost_credits INTEGER DEFAULT 0,
    created_at              TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at              TIMESTAMP,
    created_by              VARCHAR(100),
    updated_by              VARCHAR(100)
);

-- Indexes for videos table
CREATE INDEX ix_videos_user_id ON videos(user_id);
CREATE INDEX ix_videos_status ON videos(status);
CREATE INDEX ix_videos_created_at ON videos(created_at);
CREATE INDEX ix_videos_category_id ON videos(category_id);
CREATE INDEX ix_videos_template_id ON videos(template_id);
CREATE INDEX ix_videos_product_data_id ON videos(product_data_id);
CREATE INDEX ix_videos_is_public ON videos(is_public);
CREATE INDEX ix_videos_is_featured ON videos(is_featured);
CREATE UNIQUE INDEX ix_videos_freepik_task_id ON videos(freepik_task_id) WHERE freepik_task_id IS NOT NULL;
CREATE UNIQUE INDEX ix_videos_freepik_image_task_id ON videos(freepik_image_task_id) WHERE freepik_image_task_id IS NOT NULL;
```

### **video_tags**
```sql
CREATE TABLE video_tags (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name       VARCHAR(50) NOT NULL UNIQUE,
    color_hex  VARCHAR(7),
    usage_count INTEGER DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX ix_video_tags_name ON video_tags(name);
```

### **video_tag_mappings**
```sql
CREATE TABLE video_tag_mappings (
    video_id UUID NOT NULL REFERENCES videos(id) ON DELETE CASCADE,
    tag_id   UUID NOT NULL REFERENCES video_tags(id) ON DELETE CASCADE,
    PRIMARY KEY (video_id, tag_id)
);

CREATE INDEX ix_video_tag_mappings_video_id ON video_tag_mappings(video_id);
CREATE INDEX ix_video_tag_mappings_tag_id ON video_tag_mappings(tag_id);
```

### **video_shares**
```sql
CREATE TABLE video_shares (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    video_id   UUID NOT NULL REFERENCES videos(id) ON DELETE CASCADE,
    user_id    UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    platform   VARCHAR(50) NOT NULL, -- 'Instagram', 'TikTok', 'YouTube', 'Twitter', etc.
    shared_at  TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    metadata   JSONB
);

CREATE INDEX ix_video_shares_video_id ON video_shares(video_id);
CREATE INDEX ix_video_shares_user_id ON video_shares(user_id);
CREATE INDEX ix_video_shares_platform ON video_shares(platform);
CREATE INDEX ix_video_shares_shared_at ON video_shares(shared_at);
```

---

## 🛒 3. Product & E-commerce Integration

### **product_categories**
```sql
CREATE TABLE product_categories (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    parent_id   UUID REFERENCES product_categories(id),
    level       INTEGER DEFAULT 0,
    sort_order  INTEGER DEFAULT 0,
    is_active   BOOLEAN DEFAULT TRUE,
    created_at  TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMP
);

CREATE INDEX ix_product_categories_parent_id ON product_categories(parent_id);
CREATE INDEX ix_product_categories_name ON product_categories(name);
CREATE INDEX ix_product_categories_is_active ON product_categories(is_active);
```

### **product_data**
```sql
CREATE TABLE product_data (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id          UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    category_id      UUID REFERENCES product_categories(id),
    original_url     VARCHAR(1000) NOT NULL,
    domain           VARCHAR(100),
    platform         VARCHAR(50), -- 'Shopify', 'Amazon', 'Etsy', etc.
    product_id       VARCHAR(255),
    name             VARCHAR(500) NOT NULL,
    description      TEXT,
    price            DECIMAL(10,2),
    currency         VARCHAR(3) DEFAULT 'USD',
    original_price   DECIMAL(10,2),
    discount_percent INTEGER,
    brand            VARCHAR(200),
    sku              VARCHAR(100),
    availability     VARCHAR(50),
    rating           DECIMAL(3,2),
    review_count     INTEGER DEFAULT 0,
    main_image_url   VARCHAR(500),
    scraping_status  VARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'Success', 'Failed'
    scraped_at       TIMESTAMP,
    error_message    VARCHAR(1000),
    metadata         JSONB,
    created_at       TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at       TIMESTAMP,
    created_by       VARCHAR(100),
    updated_by       VARCHAR(100)
);

CREATE INDEX ix_product_data_user_id ON product_data(user_id);
CREATE INDEX ix_product_data_category_id ON product_data(category_id);
CREATE INDEX ix_product_data_original_url ON product_data(original_url);
CREATE INDEX ix_product_data_domain ON product_data(domain);
CREATE INDEX ix_product_data_platform ON product_data(platform);
CREATE INDEX ix_product_data_scraping_status ON product_data(scraping_status);
CREATE INDEX ix_product_data_created_at ON product_data(created_at);
```

### **product_images**
```sql
CREATE TABLE product_images (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_data_id UUID NOT NULL REFERENCES product_data(id) ON DELETE CASCADE,
    url             VARCHAR(500) NOT NULL,
    alt_text        VARCHAR(500),
    width           INTEGER,
    height          INTEGER,
    file_size_bytes BIGINT,
    is_primary      BOOLEAN DEFAULT FALSE,
    sort_order      INTEGER DEFAULT 0,
    created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX ix_product_images_product_data_id ON product_images(product_data_id);
CREATE INDEX ix_product_images_is_primary ON product_images(is_primary);
```

### **scraping_rules**
```sql
CREATE TABLE scraping_rules (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    domain      VARCHAR(100) NOT NULL UNIQUE,
    platform    VARCHAR(50) NOT NULL,
    selectors   JSONB NOT NULL, -- CSS selectors for different fields
    headers     JSONB,          -- Custom headers for scraping
    delay_ms    INTEGER DEFAULT 1000,
    is_active   BOOLEAN DEFAULT TRUE,
    created_at  TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  TIMESTAMP,
    updated_by  VARCHAR(100)
);

CREATE INDEX ix_scraping_rules_domain ON scraping_rules(domain);
CREATE INDEX ix_scraping_rules_platform ON scraping_rules(platform);
CREATE INDEX ix_scraping_rules_is_active ON scraping_rules(is_active);
```

---

## 💳 4. Subscription & Billing

### **subscription_plans**
```sql
CREATE TABLE subscription_plans (
    id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name                  VARCHAR(100) NOT NULL UNIQUE,
    description           TEXT,
    price_monthly         DECIMAL(8,2) NOT NULL,
    price_yearly          DECIMAL(8,2),
    currency              VARCHAR(3) DEFAULT 'USD',
    video_credits_monthly INTEGER NOT NULL DEFAULT 0,
    max_video_duration    INTEGER DEFAULT 15, -- seconds
    max_resolution        VARCHAR(20) DEFAULT '1080p',
    ai_providers          TEXT[], -- Array of allowed AI providers
    features              JSONB,   -- JSON array of features
    is_popular           BOOLEAN DEFAULT FALSE,
    is_active            BOOLEAN DEFAULT TRUE,
    trial_days           INTEGER DEFAULT 0,
    sort_order           INTEGER DEFAULT 0,
    created_at           TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at           TIMESTAMP,
    created_by           VARCHAR(100),
    updated_by           VARCHAR(100)
);

CREATE INDEX ix_subscription_plans_name ON subscription_plans(name);
CREATE INDEX ix_subscription_plans_is_active ON subscription_plans(is_active);
CREATE INDEX ix_subscription_plans_is_popular ON subscription_plans(is_popular);
```

### **user_subscriptions**
```sql
CREATE TABLE user_subscriptions (
    id                     UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id                UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    subscription_plan_id   UUID NOT NULL REFERENCES subscription_plans(id),
    status                 VARCHAR(50) NOT NULL, -- 'Active', 'Cancelled', 'Expired', 'Trial'
    billing_cycle          VARCHAR(20) NOT NULL, -- 'Monthly', 'Yearly'
    price_paid             DECIMAL(8,2) NOT NULL,
    currency               VARCHAR(3) DEFAULT 'USD',
    started_at             TIMESTAMP NOT NULL,
    ends_at                TIMESTAMP NOT NULL,
    cancelled_at           TIMESTAMP,
    trial_ends_at          TIMESTAMP,
    auto_renew             BOOLEAN DEFAULT TRUE,
    payment_method_id      UUID REFERENCES payment_methods(id),
    stripe_subscription_id VARCHAR(255),
    metadata               JSONB,
    created_at             TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at             TIMESTAMP,
    created_by             VARCHAR(100),
    updated_by             VARCHAR(100)
);

CREATE INDEX ix_user_subscriptions_user_id ON user_subscriptions(user_id);
CREATE INDEX ix_user_subscriptions_subscription_plan_id ON user_subscriptions(subscription_plan_id);
CREATE INDEX ix_user_subscriptions_status ON user_subscriptions(status);
CREATE INDEX ix_user_subscriptions_ends_at ON user_subscriptions(ends_at);
CREATE INDEX ix_user_subscriptions_stripe_subscription_id ON user_subscriptions(stripe_subscription_id);
```

### **payment_methods**
```sql
CREATE TABLE payment_methods (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id             UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    type                VARCHAR(50) NOT NULL, -- 'CreditCard', 'PayPal', 'BankAccount'
    provider            VARCHAR(50) NOT NULL, -- 'Stripe', 'PayPal'
    external_id         VARCHAR(255) NOT NULL, -- Provider's payment method ID
    last_four_digits    VARCHAR(4),
    brand               VARCHAR(50), -- 'Visa', 'MasterCard', etc.
    expires_at          TIMESTAMP,
    is_default          BOOLEAN DEFAULT FALSE,
    is_active           BOOLEAN DEFAULT TRUE,
    billing_address     JSONB,
    metadata            JSONB,
    created_at          TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at          TIMESTAMP,
    created_by          VARCHAR(100),
    updated_by          VARCHAR(100)
);

CREATE INDEX ix_payment_methods_user_id ON payment_methods(user_id);
CREATE INDEX ix_payment_methods_external_id ON payment_methods(external_id);
CREATE INDEX ix_payment_methods_is_default ON payment_methods(is_default);
CREATE INDEX ix_payment_methods_is_active ON payment_methods(is_active);
```

### **payments**
```sql
CREATE TABLE payments (
    id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id               UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    user_subscription_id  UUID REFERENCES user_subscriptions(id),
    payment_method_id     UUID REFERENCES payment_methods(id),
    amount                DECIMAL(8,2) NOT NULL,
    currency              VARCHAR(3) DEFAULT 'USD',
    status                VARCHAR(50) NOT NULL, -- 'Pending', 'Succeeded', 'Failed', 'Refunded'
    payment_intent_id     VARCHAR(255) NOT NULL UNIQUE,
    provider              VARCHAR(50) NOT NULL, -- 'Stripe', 'PayPal'
    provider_fee          DECIMAL(8,2),
    description           VARCHAR(500),
    failure_reason        VARCHAR(500),
    refunded_amount       DECIMAL(8,2) DEFAULT 0,
    metadata              JSONB,
    paid_at               TIMESTAMP,
    created_at            TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at            TIMESTAMP,
    created_by            VARCHAR(100),
    updated_by            VARCHAR(100)
);

CREATE INDEX ix_payments_user_id ON payments(user_id);
CREATE INDEX ix_payments_user_subscription_id ON payments(user_subscription_id);
CREATE INDEX ix_payments_payment_method_id ON payments(payment_method_id);
CREATE INDEX ix_payments_status ON payments(status);
CREATE INDEX ix_payments_payment_intent_id ON payments(payment_intent_id);
CREATE INDEX ix_payments_created_at ON payments(created_at);
```

### **invoices**
```sql
CREATE TABLE invoices (
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_id     UUID NOT NULL REFERENCES payments(id) ON DELETE CASCADE,
    invoice_number VARCHAR(50) NOT NULL UNIQUE,
    amount         DECIMAL(8,2) NOT NULL,
    tax_amount     DECIMAL(8,2) DEFAULT 0,
    currency       VARCHAR(3) DEFAULT 'USD',
    status         VARCHAR(50) NOT NULL, -- 'Draft', 'Sent', 'Paid', 'Cancelled'
    issued_at      TIMESTAMP NOT NULL,
    due_at         TIMESTAMP,
    paid_at        TIMESTAMP,
    pdf_url        VARCHAR(500),
    metadata       JSONB,
    created_at     TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at     TIMESTAMP
);

CREATE INDEX ix_invoices_payment_id ON invoices(payment_id);
CREATE INDEX ix_invoices_invoice_number ON invoices(invoice_number);
CREATE INDEX ix_invoices_status ON invoices(status);
CREATE INDEX ix_invoices_issued_at ON invoices(issued_at);
```

### **usage_credits**
```sql
CREATE TABLE usage_credits (
    id                   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id              UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    user_subscription_id UUID REFERENCES user_subscriptions(id),
    credit_type          VARCHAR(50) NOT NULL, -- 'VideoGeneration', 'PremiumFeature'
    amount               INTEGER NOT NULL,
    used_amount          INTEGER DEFAULT 0,
    source               VARCHAR(50) NOT NULL, -- 'Subscription', 'Purchase', 'Bonus', 'Refund'
    expires_at           TIMESTAMP,
    is_active            BOOLEAN DEFAULT TRUE,
    metadata             JSONB,
    created_at           TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at           TIMESTAMP,
    created_by           VARCHAR(100),
    updated_by           VARCHAR(100)
);

CREATE INDEX ix_usage_credits_user_id ON usage_credits(user_id);
CREATE INDEX ix_usage_credits_user_subscription_id ON usage_credits(user_subscription_id);
CREATE INDEX ix_usage_credits_credit_type ON usage_credits(credit_type);
CREATE INDEX ix_usage_credits_expires_at ON usage_credits(expires_at);
CREATE INDEX ix_usage_credits_is_active ON usage_credits(is_active);
```

---

## ⚙️ 5. User Preferences & Settings

### **user_preferences**
```sql
CREATE TABLE user_preferences (
    id                           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id                      UUID NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    default_video_resolution     VARCHAR(20) DEFAULT '1080x1920',
    default_aspect_ratio         VARCHAR(10) DEFAULT '9:16',
    default_video_duration       INTEGER DEFAULT 5,
    preferred_ai_provider        VARCHAR(50) DEFAULT 'Freepik',
    auto_save_to_library         BOOLEAN DEFAULT TRUE,
    auto_generate_thumbnails     BOOLEAN DEFAULT TRUE,
    watermark_enabled           BOOLEAN DEFAULT FALSE,
    default_video_quality        VARCHAR(20) DEFAULT 'High',
    preferred_language           VARCHAR(10) DEFAULT 'en-US',
    timezone                     VARCHAR(50) DEFAULT 'UTC',
    date_format                  VARCHAR(20) DEFAULT 'MM/DD/YYYY',
    time_format                  VARCHAR(10) DEFAULT '12h',
    theme                        VARCHAR(20) DEFAULT 'Dark',
    email_notifications_enabled BOOLEAN DEFAULT TRUE,
    push_notifications_enabled  BOOLEAN DEFAULT TRUE,
    marketing_emails_enabled     BOOLEAN DEFAULT FALSE,
    two_factor_enabled          BOOLEAN DEFAULT FALSE,
    created_at                   TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at                   TIMESTAMP,
    updated_by                   VARCHAR(100)
);

CREATE INDEX ix_user_preferences_user_id ON user_preferences(user_id);
```

### **user_interests**
```sql
CREATE TABLE user_interests (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    category   VARCHAR(100) NOT NULL, -- 'Fashion', 'Technology', 'Food', etc.
    level      INTEGER DEFAULT 1, -- 1-5 interest level
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP
);

CREATE INDEX ix_user_interests_user_id ON user_interests(user_id);
CREATE INDEX ix_user_interests_category ON user_interests(category);
CREATE UNIQUE INDEX ix_user_interests_user_category ON user_interests(user_id, category);
```

### **notification_settings**
```sql
CREATE TABLE notification_settings (
    id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id               UUID NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    video_completed       BOOLEAN DEFAULT TRUE,
    video_failed          BOOLEAN DEFAULT TRUE,
    subscription_renewal  BOOLEAN DEFAULT TRUE,
    subscription_expired  BOOLEAN DEFAULT TRUE,
    payment_failed        BOOLEAN DEFAULT TRUE,
    new_features          BOOLEAN DEFAULT TRUE,
    marketing_updates     BOOLEAN DEFAULT FALSE,
    weekly_digest         BOOLEAN DEFAULT TRUE,
    email_enabled         BOOLEAN DEFAULT TRUE,
    push_enabled          BOOLEAN DEFAULT TRUE,
    sms_enabled           BOOLEAN DEFAULT FALSE,
    created_at            TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at            TIMESTAMP,
    updated_by            VARCHAR(100)
);

CREATE INDEX ix_notification_settings_user_id ON notification_settings(user_id);
```

---

## 📊 6. Analytics & Tracking

### **video_analytics**
```sql
CREATE TABLE video_analytics (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    video_id         UUID NOT NULL REFERENCES videos(id) ON DELETE CASCADE,
    user_id          UUID REFERENCES users(id) ON DELETE SET NULL,
    event_type       VARCHAR(50) NOT NULL, -- 'View', 'Download', 'Share', 'Like'
    platform         VARCHAR(50), -- Where it was shared/viewed
    ip_address       INET,
    user_agent       VARCHAR(1000),
    referrer         VARCHAR(500),
    country_code     VARCHAR(2),
    city             VARCHAR(100),
    device_type      VARCHAR(50), -- 'Mobile', 'Desktop', 'Tablet'
    browser          VARCHAR(100),
    os               VARCHAR(100),
    session_id       VARCHAR(255),
    duration_seconds INTEGER, -- For view events
    metadata         JSONB,
    created_at       TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX ix_video_analytics_video_id ON video_analytics(video_id);
CREATE INDEX ix_video_analytics_user_id ON video_analytics(user_id);
CREATE INDEX ix_video_analytics_event_type ON video_analytics(event_type);
CREATE INDEX ix_video_analytics_platform ON video_analytics(platform);
CREATE INDEX ix_video_analytics_created_at ON video_analytics(created_at);
CREATE INDEX ix_video_analytics_country_code ON video_analytics(country_code);
```

### **user_activity**
```sql
CREATE TABLE user_activity (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    action      VARCHAR(100) NOT NULL, -- 'VideoCreated', 'ProfileUpdated', 'SubscriptionUpgraded'
    resource_type VARCHAR(50), -- 'Video', 'User', 'Subscription'
    resource_id UUID,
    description VARCHAR(500),
    ip_address  INET,
    user_agent  VARCHAR(1000),
    metadata    JSONB,
    created_at  TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX ix_user_activity_user_id ON user_activity(user_id);
CREATE INDEX ix_user_activity_action ON user_activity(action);
CREATE INDEX ix_user_activity_resource_type ON user_activity(resource_type);
CREATE INDEX ix_user_activity_created_at ON user_activity(created_at);
```

### **api_usage**
```sql
CREATE TABLE api_usage (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id         UUID REFERENCES users(id) ON DELETE SET NULL,
    endpoint        VARCHAR(200) NOT NULL,
    method          VARCHAR(10) NOT NULL,
    status_code     INTEGER NOT NULL,
    response_time_ms INTEGER,
    request_size_bytes  INTEGER,
    response_size_bytes INTEGER,
    ip_address      INET,
    user_agent      VARCHAR(1000),
    api_key_id      UUID,
    rate_limit_hit  BOOLEAN DEFAULT FALSE,
    error_message   VARCHAR(1000),
    created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX ix_api_usage_user_id ON api_usage(user_id);
CREATE INDEX ix_api_usage_endpoint ON api_usage(endpoint);
CREATE INDEX ix_api_usage_status_code ON api_usage(status_code);
CREATE INDEX ix_api_usage_created_at ON api_usage(created_at);
CREATE INDEX ix_api_usage_rate_limit_hit ON api_usage(rate_limit_hit);
```

---

## 🔄 7. Background Jobs & System Tables

### **background_jobs**
```sql
CREATE TABLE background_jobs (
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    job_type       VARCHAR(100) NOT NULL,
    job_data       JSONB NOT NULL,
    status         VARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'Running', 'Completed', 'Failed'
    priority       INTEGER DEFAULT 0,
    attempts       INTEGER DEFAULT 0,
    max_attempts   INTEGER DEFAULT 3,
    scheduled_at   TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    started_at     TIMESTAMP,
    completed_at   TIMESTAMP,
    error_message  VARCHAR(2000),
    created_at     TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at     TIMESTAMP
);

CREATE INDEX ix_background_jobs_job_type ON background_jobs(job_type);
CREATE INDEX ix_background_jobs_status ON background_jobs(status);
CREATE INDEX ix_background_jobs_scheduled_at ON background_jobs(scheduled_at);
CREATE INDEX ix_background_jobs_priority ON background_jobs(priority);
```

### **system_settings**
```sql
CREATE TABLE system_settings (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    key        VARCHAR(100) NOT NULL UNIQUE,
    value      TEXT NOT NULL,
    data_type  VARCHAR(20) DEFAULT 'string', -- 'string', 'number', 'boolean', 'json'
    category   VARCHAR(50),
    description TEXT,
    is_public  BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    updated_by VARCHAR(100)
);

CREATE INDEX ix_system_settings_key ON system_settings(key);
CREATE INDEX ix_system_settings_category ON system_settings(category);
CREATE INDEX ix_system_settings_is_public ON system_settings(is_public);
```

---

## 📚 8. Data Types & Enums Reference

### **Common Enums:**
- **VideoStatus**: `Pending`, `Processing`, `GeneratingImage`, `GeneratingVideo`, `Completed`, `Failed`, `Cancelled`
- **VideoInputType**: `Text`, `Image`, `Url`
- **SubscriptionStatus**: `Active`, `Cancelled`, `Expired`, `Trial`, `PastDue`
- **PaymentStatus**: `Pending`, `Succeeded`, `Failed`, `Refunded`, `Cancelled`
- **UserRole**: `User`, `Admin`, `Moderator`, `Support`
- **NotificationType**: `Info`, `Warning`, `Error`, `Success`

### **Common JSON Structures:**

**VideoMetadata (JSONB):**
```json
{
  "width": 1080,
  "height": 1920,
  "frameRate": 30.0,
  "format": "mp4",
  "codec": "h264",
  "bitrate": 2000000,
  "duration": 5.2,
  "fileSize": 15728640
}
```

**ProductData.metadata (JSONB):**
```json
{
  "variants": [...],
  "specifications": {...},
  "shipping": {...},
  "seo": {...}
}
```

**SubscriptionPlan.features (JSONB):**
```json
[
  "HD Video Generation",
  "Premium Templates",
  "Advanced AI Models",
  "Priority Processing",
  "Commercial License"
]
```

---

## 🔧 Database Constraints & Business Rules

### **Key Constraints:**
1. **User Email Uniqueness**: Each email can only be associated with one active user account
2. **Active Subscription Limit**: Users can have only one active subscription at a time
3. **Credit Usage**: Users cannot use more credits than available
4. **Video Generation Limits**: Based on subscription tier
5. **File Size Limits**: Based on subscription tier
6. **API Rate Limits**: Stored and enforced at application level

### **Cascade Rules:**
- **User Deletion**: Cascade to all user-related data (videos, preferences, etc.)
- **Video Deletion**: Cascade to analytics and shares
- **Subscription Cancellation**: Preserve historical payment data

### **Data Retention:**
- **User Activity**: 2 years
- **Video Analytics**: 1 year
- **API Usage**: 90 days
- **Background Jobs**: 30 days (completed/failed)

---

## 💾 Recommended Database Configuration

### **PostgreSQL Extensions:**
```sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";    -- For text search
CREATE EXTENSION IF NOT EXISTS "btree_gin";  -- For JSONB indexing
```

### **Performance Considerations:**
1. **Connection Pooling**: Use PgBouncer or similar
2. **Read Replicas**: For analytics queries
3. **Partitioning**: Consider partitioning large tables (video_analytics, user_activity) by date
4. **JSONB Indexing**: Create GIN indexes on frequently queried JSONB fields
5. **Archival Strategy**: Archive old data to reduce table sizes

This schema provides a solid foundation for the EcomVideo AI application with room for future expansion and optimization. 