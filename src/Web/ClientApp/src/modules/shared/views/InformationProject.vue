<template>
  <DefaultLayout>
    <div class="information-project-page">
      <div class="cyborg-container">
        <!-- Header Section -->
        <div class="page-header">
          <h1 class="page-title">
            <Icon icon="mdi:information" class="title-icon" />
            Information Project
          </h1>
          <p class="page-subtitle">RoomEnglish is a modern English vocabulary learning application, built with Clean Architecture, integrating advanced technologies to deliver the best learning experience for users.</p>
        </div>

        <!-- Content Section -->
        <div class="content-grid">
          <!-- Technology Stack -->
          <div class="info-card">
            <div class="card-header">
              <Icon icon="mdi:code-tags" class="card-icon" />
              <h2 class="card-title">Technology Stack</h2>
            </div>
            <div class="card-content">
              <div class="tech-section">
                <h3 class="section-title">Backend</h3>
                <ul class="tech-list">
                  <li><strong>Framework:</strong> ASP.NET Core (Web API)</li>
                  <li><strong>ORM:</strong> Entity Framework Core</li>
                  <li><strong>Database:</strong> SQL Server</li>
                  <li><strong>Authentication:</strong> ASP.NET Core Identity + Google OAuth</li>
                  <li><strong>Validation:</strong> FluentValidation</li>
                  <li><strong>Mapping:</strong> AutoMapper</li>
                  <li><strong>API Docs:</strong> NSwag (OpenAPI/Swagger)</li>
                  <li><strong>AI Integration:</strong> OpenAI SDK</li>
                </ul>
              </div>
              <div class="tech-section">
                <h3 class="section-title">Frontend</h3>
                <ul class="tech-list">
                  <li><strong>Framework:</strong> Vue 3 (Composition API)</li>
                  <li><strong>Language:</strong> TypeScript</li>
                  <li><strong>Build Tool:</strong> Vite</li>
                  <li><strong>UI Framework:</strong> PrimeVue + Tailwind CSS</li>
                  <li><strong>State Management:</strong> Pinia</li>
                  <li><strong>Routing:</strong> Vue Router</li>
                  <li><strong>HTTP Client:</strong> Axios</li>
                  <li><strong>i18n:</strong> Vue I18n</li>
                  <li><strong>Testing:</strong> Vitest</li>
                </ul>
              </div>
              <div class="tech-section">
                <h3 class="section-title">DevOps & CI/CD</h3>
                <ul class="tech-list">
                  <li><strong>CI/CD:</strong> GitHub Actions + Azure Pipelines</li>
                  <li><strong>Cloud:</strong> Azure (App Service)</li>
                </ul>
              </div>
            </div>
          </div>

          <!-- Features -->
          <div class="info-card">
            <div class="card-header">
              <Icon icon="mdi:star-circle" class="card-icon" />
              <h2 class="card-title">Key Features</h2>
            </div>
            <div class="card-content">
              <div class="features-grid">
                <div class="feature-item">
                  <Icon icon="mdi:book-alphabet" class="feature-icon" />
                  <h4 class="feature-title">Vocabulary Learning</h4>
                  <p class="feature-desc">Category-based vocabulary learning system with example illustrations</p>
                </div>
                <div class="feature-item">
                  <Icon icon="mdi:microphone" class="feature-icon" />
                  <h4 class="feature-title">Text-to-Speech</h4>
                  <p class="feature-desc">Pronounce vocabulary with advanced TTS technology</p>
                </div>
                <div class="feature-item">
                  <Icon icon="mdi:file-document-edit" class="feature-icon" />
                  <h4 class="feature-title">Dictation</h4>
                  <p class="feature-desc">Practice spelling to improve listening and writing skills</p>
                </div>
                <div class="feature-item">
                  <Icon icon="mdi:chart-line" class="feature-icon" />
                  <h4 class="feature-title">Progress Tracking</h4>
                  <p class="feature-desc">Manage and track your learning progress</p>
                </div>
                <div class="feature-item">
                  <Icon icon="mdi:account-multiple" class="feature-icon" />
                  <h4 class="feature-title">Multi-factor Authentication</h4>
                  <p class="feature-desc">Sign in with Email/Password or Google OAuth</p>
                </div>
                <div class="feature-item">
                  <Icon icon="mdi:file-excel" class="feature-icon" />
                  <h4 class="feature-title">Import/Export Excel</h4>
                  <p class="feature-desc">Import and export vocabulary data via Excel files</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Architecture -->
          <div class="info-card">
            <div class="card-header">
              <Icon icon="mdi:sitemap" class="card-icon" />
              <h2 class="card-title">System Architecture</h2>
            </div>
            <div class="card-content">
              <div class="architecture-diagram">
                <div class="arch-layer">
                  <div class="layer-name">Presentation Layer</div>
                  <div class="layer-tech">Vue.js 3 + TypeScript + PrimeVue</div>
                </div>
                <div class="arch-arrow">↓</div>
                <div class="arch-layer">
                  <div class="layer-name">Application Layer</div>
                  <div class="layer-tech">CQRS Pattern + MediatR + FluentValidation</div>
                </div>
                <div class="arch-arrow">↓</div>
                <div class="arch-layer">
                  <div class="layer-name">Domain Layer</div>
                  <div class="layer-tech">Entities + Value Objects + Domain Events</div>
                </div>
                <div class="arch-arrow">↓</div>
                <div class="arch-layer">
                  <div class="layer-name">Infrastructure Layer</div>
                  <div class="layer-tech">EF Core + Identity + External Services</div>
                </div>
                <div class="arch-arrow">↓</div>
                <div class="arch-layer">
                  <div class="layer-name">Database</div>
                  <div class="layer-tech">SQL Server</div>
                </div>
              </div>
            </div>
          </div>

          <!-- Development Info -->
          <div class="info-card">
            <div class="card-header">
              <Icon icon="mdi:code-braces" class="card-icon" />
              <h2 class="card-title">Development Information</h2>
            </div>
            <div class="card-content">
              <div class="dev-info">
                <div class="info-row">
                  <Icon icon="mdi:source-branch" class="info-icon" />
                  <div>
                    <strong>Version Control:</strong> Git
                  </div>
                </div>
                <div class="info-row">
                  <Icon icon="mdi:github" class="info-icon" />
                  <div>
                    <strong>Repository:</strong> GitHub
                  </div>
                </div>
                <div class="info-row">
                  <Icon icon="mdi:cloud" class="info-icon" />
                  <div>
                    <strong>Cloud Platform:</strong> Microsoft Azure
                  </div>
                </div>
                <div class="info-row">
                  <Icon icon="mdi:package-variant" class="info-icon" />
                  <div>
                    <strong>Package Manager:</strong> NuGet (Backend) + npm (Frontend)
                  </div>
                </div>
                <div class="info-row">
                  <Icon icon="mdi:rocket" class="info-icon" />
                  <div>
                    <strong>Deployment:</strong> Azure App Service with automated CI/CD
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </DefaultLayout>
</template>

<script setup lang="ts">
import DefaultLayout from '../layouts/DefaultLayout.vue'
import { Icon } from '@iconify/vue'
</script>

<style scoped>
.information-project-page {
  min-height: calc(100vh - 80px);
  padding: 40px 0;
  background: linear-gradient(135deg, var(--primary-bg) 0%, var(--secondary-bg) 100%);
}

.cyborg-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

/* Header */
.page-header {
  text-align: center;
  margin-bottom: 50px;
}

.page-title {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 15px;
  font-size: 48px;
  font-weight: 700;
  background: var(--gradient-primary);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin-bottom: 15px;
}

.title-icon {
  width: 48px;
  height: 48px;
  color: var(--accent-pink);
}

.page-subtitle {
  font-size: 20px;
  color: var(--text-secondary);
  font-weight: 400;
}

/* Content Grid */
.content-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
  gap: 30px;
  margin-top: 40px;
}

/* Info Cards */
.info-card {
  background: var(--card-bg);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  padding: 30px;
  box-shadow: var(--shadow-card);
  transition: all 0.3s ease;
}

.info-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 30px rgba(236, 96, 144, 0.2);
  border-color: var(--accent-pink);
}

.card-header {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-bottom: 20px;
  padding-bottom: 15px;
  border-bottom: 2px solid var(--border-color);
}

.card-icon {
  width: 32px;
  height: 32px;
  color: var(--accent-pink);
}

.card-title {
  font-size: 24px;
  font-weight: 600;
  color: var(--text-primary);
  margin: 0;
}

.card-content {
  color: var(--text-secondary);
  line-height: 1.8;
}

.card-description {
  font-size: 16px;
  margin: 0;
}

/* Tech Section */
.tech-section {
  margin-bottom: 25px;
}

.tech-section:last-child {
  margin-bottom: 0;
}

.section-title {
  font-size: 18px;
  font-weight: 600;
  color: var(--accent-pink);
  margin-bottom: 12px;
}

.tech-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.tech-list li {
  padding: 8px 0;
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
  font-size: 14px;
}

.tech-list li:last-child {
  border-bottom: none;
}

.tech-list strong {
  color: var(--text-primary);
  margin-right: 8px;
}

/* Features Grid */
.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 20px;
}

.feature-item {
  text-align: center;
  padding: 20px;
  background: var(--secondary-bg);
  border-radius: var(--radius-md);
  border: 1px solid var(--border-color);
  transition: all 0.3s ease;
}

.feature-item:hover {
  border-color: var(--accent-pink);
  transform: scale(1.05);
}

.feature-icon {
  width: 40px;
  height: 40px;
  color: var(--accent-pink);
  margin-bottom: 12px;
}

.feature-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 8px;
}

.feature-desc {
  font-size: 14px;
  color: var(--text-secondary);
  margin: 0;
  line-height: 1.6;
}

/* Architecture Diagram */
.architecture-diagram {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 15px;
  padding: 20px;
}

.arch-layer {
  width: 100%;
  padding: 20px;
  background: var(--secondary-bg);
  border: 2px solid var(--border-color);
  border-radius: var(--radius-md);
  text-align: center;
  transition: all 0.3s ease;
}

.arch-layer:hover {
  border-color: var(--accent-pink);
  background: rgba(236, 96, 144, 0.1);
}

.layer-name {
  font-size: 18px;
  font-weight: 600;
  color: var(--accent-pink);
  margin-bottom: 8px;
}

.layer-tech {
  font-size: 14px;
  color: var(--text-secondary);
}

.arch-arrow {
  font-size: 24px;
  color: var(--accent-pink);
  font-weight: bold;
}

/* Development Info */
.dev-info {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.info-row {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: var(--secondary-bg);
  border-radius: var(--radius-md);
  border-left: 3px solid var(--accent-pink);
}

.info-icon {
  width: 24px;
  height: 24px;
  color: var(--accent-pink);
  flex-shrink: 0;
}

.info-row strong {
  color: var(--text-primary);
  margin-right: 8px;
}

/* Responsive */
@media (max-width: 768px) {
  .page-title {
    font-size: 32px;
  }

  .title-icon {
    width: 32px;
    height: 32px;
  }

  .content-grid {
    grid-template-columns: 1fr;
    gap: 20px;
  }

  .features-grid {
    grid-template-columns: 1fr;
  }

  .info-card {
    padding: 20px;
  }
}
</style>
