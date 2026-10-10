<?php
// config.example.php - Database configuration template
// WARNING: Do NOT commit actual database passwords to version control!

$host = getenv('DB_HOST') ?: 'localhost';
$dbname = getenv('DB_NAME') ?: 'sonari_db';
$username = getenv('DB_USER') ?: 'sonari_user';
$password = getenv('DB_PASS') ?: 'YOUR_DATABASE_PASSWORD_HERE';

try {
    $pdo = new PDO("mysql:host=$host;dbname=$dbname;charset=utf8mb4", $username, $password);
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    $pdo->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_ASSOC);

    // Test connection
    $stmt = $pdo->query("SELECT 1");
} catch (PDOException $e) {
    die("Database connection failed. Please verify credentials in config.php: " . $e->getMessage());
}

if (session_status() === PHP_SESSION_NONE) {
    session_start();
}
?>