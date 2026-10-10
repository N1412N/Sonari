# Sonari ASL Web App (sonari.8offer.com)

This folder contains the complete, production-ready web application source code and media assets powering the live web portal at [https://sonari.8offer.com/](https://sonari.8offer.com/).

## 📁 File Manifest
- **index.html**: The complete client-side ASL Gesture Speller app (HTML5, responsive CSS, MediaPipe Hand tracking, and inlined neural network weights for zero-server inference).
- **can_you_make_a_video_on_this_i.mp4**: Ambient looping background video for index.html.
- **login.php**: User authentication and registration portal interface with responsive glassmorphism UI.
- **S__10149890.jpg**: Full-screen background image for login.php.
- **S__10264581.jpg**: Official Sonari logo image displayed on login.php.
- **config.example.php**: Sanitized PDO MySQL database configuration template with environment variable support.

> **Security Note:** Production database passwords and backend authentication secrets (uth.php) are intentionally excluded from version control to prevent credential leakage.
