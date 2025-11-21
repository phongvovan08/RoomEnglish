<template>
    <div class="cyborg-dashboard fade-in-up">
        <!-- Main Banner -->
        <div class="main-banner cyborg-card hover-glow">
            <div class="cyborg-grid cyborg-grid-2" style="align-items: center;">
                <div class="banner-content">
                    <h6>Welcome To</h6>
                    <p>Browse your tasks, check weather, and manage everything in one place with our awesome gaming-style interface.</p>
                    <div class="main-button">
                        <router-link :to="Routes.TodoLists.children.Create.path" class="cyborg-btn">
                            <Icon icon="mdi:plus-circle" class="w-5 h-5 mr-2" />
                            Create New List
                        </router-link>
                    </div>
                </div>
                <div class="banner-image">
                    <Icon icon="mdi:gamepad-variant" class="gaming-icon" />
                </div>
            </div>
        </div>

        <!-- Most Popular Games (Quick Actions) -->
        <div class="most-popular cyborg-section">

            <div class="cyborg-grid cyborg-grid-4">
                <div class="game-item hover-glow">
                    <div class="thumb">
                        <router-link :to="Routes.TodoLists.children.Create.path">
                            <Icon icon="mdi:plus-circle" class="game-thumb-icon" />
                        </router-link>
                    </div>
                    <div class="down-content">
                        <h4>Create Todo List</h4>
                        <span>Quick Action</span>
                        <ul>
                            <li><i class="fa fa-star"></i> 4.8</li>
                            <li><i class="fa fa-download"></i> 2.3M</li>
                        </ul>
                    </div>
                </div>

                <div class="game-item hover-glow">
                    <div class="thumb">
                        <router-link :to="Routes.TodoItems.children.Create.path">
                            <Icon icon="mdi:checkbox-marked-circle" class="game-thumb-icon" />
                        </router-link>
                    </div>
                    <div class="down-content">
                        <h4>Add Todo Item</h4>
                        <span>Quick Action</span>
                        <ul>
                            <li><i class="fa fa-star"></i> 4.9</li>
                            <li><i class="fa fa-download"></i> 1.8M</li>
                        </ul>
                    </div>
                </div>

                <div class="game-item hover-glow">
                    <div class="thumb">
                        <router-link :to="Routes.TodoLists.children.List.path">
                            <Icon icon="mdi:view-list" class="game-thumb-icon" />
                        </router-link>
                    </div>
                    <div class="down-content">
                        <h4>View All Lists</h4>
                        <span>Browse</span>
                        <ul>
                            <li><i class="fa fa-star"></i> 4.7</li>
                            <li><i class="fa fa-download"></i> 1.2M</li>
                        </ul>
                    </div>
                </div>

                <div class="game-item hover-glow">
                    <div class="thumb">
                        <router-link :to="Routes.WeatherForecasts.path">
                            <Icon icon="mdi:weather-partly-cloudy" class="game-thumb-icon" />
                        </router-link>
                    </div>
                    <div class="down-content">
                        <h4>Weather Forecast</h4>
                        <span>Information</span>
                        <ul>
                            <li><i class="fa fa-star"></i> 4.6</li>
                            <li><i class="fa fa-download"></i> 980K</li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>

        <!-- Profile Stats -->
        <div class="profile-section cyborg-section">
            <div class="cyborg-card">
                <div class="section-heading">
                    <h6>Your Profile</h6>
                    <h4>Gaming <em>Statistics</em></h4>
                </div>

                <div class="profile-stats">
                    <div class="stat-item">
                        <span class="value">{{ stats.completedTasks || 45 }}</span>
                        <span class="label">Completed Tasks</span>
                    </div>

                    <div class="stat-item">
                        <span class="value">{{ stats.streak || 7 }}</span>
                        <span class="label">Day Streak</span>
                    </div>

                    <div class="stat-item">
                        <span class="value">{{ stats.achievements || 12 }}</span>
                        <span class="label">Achievements</span>
                    </div>

                    <div class="stat-item">
                        <span class="value">{{ stats.level || 15 }}</span>
                        <span class="label">Level</span>
                    </div>
                </div>
            </div>
        </div>

        <!-- Recent Activity -->
        <div class="recent-activity cyborg-section">
            <div class="section-heading">
                <h6>Recent</h6>
                <h4>Activity <em>Stream</em></h4>
            </div>

            <div class="cyborg-card">
                <div class="activity-stream">
                    <div class="activity-item slide-in-left">
                        <div class="activity-icon created">
                            <Icon icon="mdi:plus-circle" />
                        </div>
                        <div class="activity-content">
                            <h5>New Todo List Created</h5>
                            <p>You created "Weekly Planning" list</p>
                            <span class="time">2 hours ago</span>
                        </div>
                    </div>

                    <div class="activity-item slide-in-left">
                        <div class="activity-icon completed">
                            <Icon icon="mdi:check-circle" />
                        </div>
                        <div class="activity-content">
                            <h5>Task Completed</h5>
                            <p>Completed "Review project documentation"</p>
                            <span class="time">4 hours ago</span>
                        </div>
                    </div>

                    <div class="activity-item slide-in-left">
                        <div class="activity-icon weather">
                            <Icon icon="mdi:weather-cloudy" />
                        </div>
                        <div class="activity-content">
                            <h5>Weather Checked</h5>
                            <p>Viewed today's weather forecast</p>
                            <span class="time">1 day ago</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
    import { Routes } from '@/router/constants'

    // Mock data
    const stats = reactive({
      todoLists: 12,
      todoItems: 45,
      temperature: 22,
      posts: 8,
      completedTasks: 45,
      streak: 7,
      achievements: 12,
      level: 15
    })

    // Animate stats on mount
    onMounted(() => {
      // Add staggered animations to game items
      const gameItems = document.querySelectorAll('.game-item')
      gameItems.forEach((item, index) => {
        (item as HTMLElement).style.animationDelay = `${index * 0.1}s`
      })

      // Add staggered animations to activity items
      const activityItems = document.querySelectorAll('.activity-item')
      activityItems.forEach((item, index) => {
        (item as HTMLElement).style.animationDelay = `${index * 0.2}s`
      })
    })
</script>

<style scoped>
    /* Dashboard Specific Styles */
    .cyborg-dashboard {
        display: flex;
        flex-direction: column;
        gap: 60px;
        padding: 2rem 1rem;
    }

    /* Main Banner */
    .main-banner {
        padding: 60px;
        position: relative;
        overflow: hidden;
    }

        .main-banner::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: linear-gradient(45deg, rgba(236, 96, 144, 0.1) 0%, rgba(133, 82, 244, 0.1) 100%);
            z-index: -1;
        }

    .banner-content h6 {
        font-size: 15px;
        color: var(--accent-pink);
        font-weight: 400;
        margin-bottom: 15px;
        text-transform: uppercase;
        letter-spacing: 1px;
    }

    .banner-content h4 {
        font-size: 36px;
        font-weight: 700;
        line-height: 54px;
        margin-bottom: 25px;
    }

        .banner-content h4 em {
            font-style: normal;
            color: var(--accent-pink);
        }

    .banner-content p {
        color: var(--text-secondary);
        line-height: 28px;
        margin-bottom: 30px;
        font-size: 16px;
    }

    .main-button {
        margin-top: 30px;
    }

    .banner-image {
        display: flex;
        justify-content: center;
        align-items: center;
    }

    .gaming-icon {
        width: 200px;
        height: 200px;
        color: var(--accent-pink);
        opacity: 0.3;
        animation: float 3s ease-in-out infinite;
    }

    @keyframes float {
        0%, 100% {
            transform: translateY(0px);
        }

        50% {
            transform: translateY(-20px);
        }
    }

    /* Gaming Library Stats */
    .gaming-library-item {
        animation: slideInUp 0.6s ease-out both;
    }

        .gaming-library-item .left-image {
            display: flex;
            align-items: center;
            justify-content: center;
            width: 80px;
            height: 80px;
            border-radius: var(--radius-sm);
        }

    .stat-icon {
        width: 60px;
        height: 60px;
        border-radius: var(--radius-sm);
        display: flex;
        align-items: center;
        justify-content: center;
        color: white;
    }

        .stat-icon.todo-lists {
            background: var(--gradient-primary);
        }

        .stat-icon.todo-items {
            background: var(--gradient-secondary);
        }

        .stat-icon.weather {
            background: linear-gradient(105deg, #4facfe 0%, #00f2fe 100%);
        }

        .stat-icon.posts {
            background: linear-gradient(105deg, #43e97b 0%, #38f9d7 100%);
        }

    /* Game Items */
    .game-item {
        animation: fadeInUp 0.6s ease-out both;
        max-width: 100%;
    }

    /* Force 4-column grid layout */
    .most-popular .cyborg-grid-4 {
        grid-template-columns: repeat(4, minmax(0, 1fr));
    }

    .game-item .thumb {
        height: 200px;
        background: var(--gradient-secondary);
        border-radius: var(--radius-md) var(--radius-md) 0 0;
        display: flex;
        align-items: center;
        justify-content: center;
        position: relative;
        overflow: hidden;
    }

        .game-item .thumb::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: linear-gradient(45deg, rgba(0,0,0,0.3), transparent);
        }

    .game-thumb-icon {
        width: 80px;
        height: 80px;
        color: white;
        z-index: 1;
        transition: var(--transition-normal);
    }

    .game-item:hover .game-thumb-icon {
        transform: scale(1.1) rotate(5deg);
    }

    .game-item .down-content ul {
        display: flex;
        gap: 15px;
        margin-top: 20px;
    }

        .game-item .down-content ul li {
            display: flex;
            align-items: center;
            gap: 5px;
            color: var(--text-muted);
            font-size: 14px;
        }

            .game-item .down-content ul li i {
                color: #ffcc00;
            }

    /* Profile Stats */
    .profile-stats {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
        gap: 30px;
        margin-top: 30px;
    }

    .stat-item {
        text-align: center;
        padding: 20px;
        background: var(--secondary-bg);
        border-radius: var(--radius-sm);
        border: 1px solid var(--border-color);
        transition: var(--transition-normal);
    }

        .stat-item:hover {
            transform: translateY(-5px);
            box-shadow: var(--shadow-hover);
        }

    /* Activity Stream */
    .activity-stream {
        display: flex;
        flex-direction: column;
        gap: 20px;
    }

    .activity-item {
        display: flex;
        align-items: center;
        padding: 20px;
        background: var(--secondary-bg);
        border-radius: var(--radius-sm);
        border: 1px solid var(--border-color);
        transition: var(--transition-normal);
        animation: slideInLeft 0.6s ease-out both;
    }

        .activity-item:hover {
            transform: translateX(10px);
            box-shadow: var(--shadow-hover);
        }

    .activity-icon {
        width: 50px;
        height: 50px;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        margin-right: 20px;
        flex-shrink: 0;
    }

        .activity-icon.created {
            background: rgba(16, 185, 129, 0.2);
            color: #10b981;
        }

        .activity-icon.completed {
            background: rgba(59, 130, 246, 0.2);
            color: #3b82f6;
        }

        .activity-icon.weather {
            background: rgba(245, 158, 11, 0.2);
            color: #f59e0b;
        }

    .activity-content h5 {
        font-size: 18px;
        margin-bottom: 5px;
        color: var(--text-primary);
    }

    .activity-content p {
        color: var(--text-secondary);
        margin-bottom: 10px;
        font-size: 14px;
    }

    .activity-content .time {
        color: var(--text-muted);
        font-size: 12px;
    }

    /* Animations */
    @keyframes slideInUp {
        from {
            opacity: 0;
            transform: translateY(40px);
        }

        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    @keyframes slideInLeft {
        from {
            opacity: 0;
            transform: translateX(-40px);
        }

        to {
            opacity: 1;
            transform: translateX(0);
        }
    }

    @keyframes fadeInUp {
        from {
            opacity: 0;
            transform: translateY(30px);
        }

        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    /* Responsive Design */
    @media (max-width: 1024px) and (min-width: 769px) {
        .most-popular .cyborg-grid-4 {
            grid-template-columns: repeat(2, minmax(0, 1fr));
        }
    }

    @media (max-width: 768px) {
        .main-banner {
            padding: 40px 30px;
        }

        .banner-content h4 {
            font-size: 28px;
            line-height: 42px;
        }

        .gaming-icon {
            width: 150px;
            height: 150px;
        }

        .cyborg-grid-2,
        .cyborg-grid-4,
        .most-popular .cyborg-grid-4 {
            grid-template-columns: 1fr;
        }

        .profile-stats {
            grid-template-columns: repeat(2, 1fr);
            gap: 20px;
        }

        .gaming-library-item {
            flex-direction: column;
            text-align: center;
        }

            .gaming-library-item .left-image {
                margin-right: 0;
                margin-bottom: 20px;
            }

        .activity-item {
            flex-direction: column;
            text-align: center;
        }

        .activity-icon {
            margin-right: 0;
            margin-bottom: 15px;
        }
    }

    @media (max-width: 480px) {
        .cyborg-dashboard {
            gap: 40px;
        }

        .profile-stats {
            grid-template-columns: 1fr;
        }

        .banner-content h4 {
            font-size: 24px;
            line-height: 36px;
        }
    }

    /* Accessibility */
    @media (prefers-reduced-motion: reduce) {
        .gaming-icon,
        .activity-item,
        .game-item,
        .gaming-library-item {
            animation: none;
        }
    }
</style>