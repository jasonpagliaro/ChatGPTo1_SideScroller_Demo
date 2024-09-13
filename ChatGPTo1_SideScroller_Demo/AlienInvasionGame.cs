using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Timer = System.Windows.Forms.Timer;

namespace AlienInvasionGame
{
    public class GameForm : Form
    {
        // Fields
        private Timer _gameTimer;
        private Player _player;
        private List<Alien> _aliens;
        private List<Platform> _platforms;
        private List<Obstacle> _obstacles;
        private PowerUp _powerUp;
        private Background _background;
        private Random _random;
        private int _score;
        private bool _isGameOver;
        private bool _isCountdownOver;
        private int _countdownTimer;
        private int _consecutiveAliens;
        private int _gapCounter;
        private bool _leftKeyPressed;
        private bool _rightKeyPressed;

        // Constructor
        public GameForm()
        {
            InitializeForm();
            InitializeGame();
            InitializeEvents();
        }

        private void InitializeForm()
        {
            this.Text = "Alien Invasion";
            this.DoubleBuffered = true;
            this.Width = 800;
            this.Height = 600;
            this.BackColor = Color.Black;
        }

        private void InitializeGame()
        {
            _random = new Random();
            _player = new Player();
            _aliens = new List<Alien>();
            _platforms = new List<Platform>();
            _obstacles = new List<Obstacle>();
            _background = new Background(this.Width, this.Height);
            _powerUp = null;
            _score = 0;
            _isGameOver = false;
            _isCountdownOver = false;
            _countdownTimer = 5000;
            _consecutiveAliens = 0;
            _gapCounter = 0;
            _leftKeyPressed = false;
            _rightKeyPressed = false;

            GeneratePlatforms();

            _gameTimer = new Timer();
            _gameTimer.Interval = 16;
            _gameTimer.Tick += UpdateGame;
            _gameTimer.Start();
        }

        private void InitializeEvents()
        {
            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;
        }

        private void GeneratePlatforms()
        {
            _platforms.Add(new Platform(0, 500, this.Width));
            _platforms.Add(new Platform(150, 400, this.Width / 2));
            _platforms.Add(new Platform(300, 300, this.Width / 3));
            _platforms.Add(new Platform(100, 200, this.Width / 4));
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!_isGameOver && _isCountdownOver)
            {
                if (e.KeyCode == Keys.Space)
                {
                    _player.StartJump();
                }
                else if (e.KeyCode == Keys.Left)
                {
                    _leftKeyPressed = true;
                }
                else if (e.KeyCode == Keys.Right)
                {
                    _rightKeyPressed = true;
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                RestartGame();
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (!_isGameOver && _isCountdownOver)
            {
                if (e.KeyCode == Keys.Space)
                {
                    _player.EndJump();
                }
                else if (e.KeyCode == Keys.Left)
                {
                    _leftKeyPressed = false;
                }
                else if (e.KeyCode == Keys.Right)
                {
                    _rightKeyPressed = false;
                }
            }
        }

        private void UpdateGame(object sender, EventArgs e)
        {
            if (_isGameOver)
                return;

            if (_countdownTimer > 0)
            {
                _countdownTimer -= _gameTimer.Interval;
            }
            else
            {
                _isCountdownOver = true;
            }

            if (_isCountdownOver)
            {
                _player.Update(_platforms, _leftKeyPressed, _rightKeyPressed);
                UpdateAliens();
                UpdateObstacles();
                UpdatePowerUp();
                CheckCollisions();
            }

            _background.Update();
            this.Invalidate();
        }

        private void UpdateAliens()
        {
            if (_gapCounter > 0)
            {
                _gapCounter--;
            }
            else
            {
                if (_consecutiveAliens < 2)
                {
                    if (_random.Next(100) < 2)
                    {
                        Platform platform = _platforms[_random.Next(_platforms.Count)];
                        _aliens.Add(new Alien(this.Width, platform.Y - 40, _random));
                        _consecutiveAliens++;
                    }
                }
                else
                {
                    _gapCounter = 50;
                    _consecutiveAliens = 0;
                }
            }

            for (int i = _aliens.Count - 1; i >= 0; i--)
            {
                _aliens[i].Update();
                if (_aliens[i].X + _aliens[i].Width < 0)
                {
                    _aliens.RemoveAt(i);
                    _score += 10;
                }
            }
        }

        private void UpdateObstacles()
        {
            if (_random.Next(100) < 2)
            {
                Platform platform = _platforms[_random.Next(_platforms.Count)];
                _obstacles.Add(new Obstacle(this.Width, platform.Y - 20, _random));
            }

            for (int i = _obstacles.Count - 1; i >= 0; i--)
            {
                _obstacles[i].Update();
                if (_obstacles[i].X + _obstacles[i].Width < 0)
                {
                    _obstacles.RemoveAt(i);
                }
            }
        }

        private void UpdatePowerUp()
        {
            if (_powerUp == null && _random.Next(1000) < 2)
            {
                Platform platform = _platforms[_random.Next(_platforms.Count)];
                _powerUp = new PowerUp(this.Width, platform.Y - 30);
            }

            if (_powerUp != null)
            {
                _powerUp.Update();
                if (_powerUp.X + _powerUp.Width < 0)
                {
                    _powerUp = null;
                }
            }
        }

        private void CheckCollisions()
        {
            Rectangle playerRect = _player.GetBounds();

            foreach (var alien in _aliens)
            {
                if (playerRect.IntersectsWith(alien.GetBounds()))
                {
                    GameOver();
                    return;
                }
            }

            foreach (var obstacle in _obstacles)
            {
                if (playerRect.IntersectsWith(obstacle.GetBounds()))
                {
                    GameOver();
                    return;
                }
            }

            if (_powerUp != null && playerRect.IntersectsWith(_powerUp.GetBounds()))
            {
                _player.ActivatePowerUp();
                _powerUp = null;
            }
        }

        private void GameOver()
        {
            _isGameOver = true;
            _gameTimer.Stop();
            this.Invalidate();
        }

        private void RestartGame()
        {
            _aliens.Clear();
            _obstacles.Clear();
            _player = new Player();
            _background = new Background(this.Width, this.Height);
            _score = 0;
            _isGameOver = false;
            _isCountdownOver = false;
            _countdownTimer = 5000;
            _consecutiveAliens = 0;
            _gapCounter = 0;
            _powerUp = null;
            _leftKeyPressed = false;
            _rightKeyPressed = false;
            _gameTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            _background.Draw(g);

            foreach (var platform in _platforms)
            {
                platform.Draw(g);
            }

            _player.Draw(g);

            foreach (var alien in _aliens)
            {
                alien.Draw(g);
            }

            foreach (var obstacle in _obstacles)
            {
                obstacle.Draw(g);
            }

            if (_powerUp != null)
            {
                _powerUp.Draw(g);
            }

            using (Font scoreFont = new Font("Arial", 16))
            {
                g.DrawString("Score: " + _score, scoreFont, Brushes.White, 10, 10);
            }

            if (!_isCountdownOver)
            {
                using (Font countdownFont = new Font("Arial", 32, FontStyle.Bold))
                {
                    int countdownSeconds = Math.Max(0, _countdownTimer / 1000);
                    g.DrawString(countdownSeconds.ToString(), countdownFont, Brushes.Yellow,
                        this.Width / 2 - 20, 20);
                }
            }

            if (_isGameOver)
            {
                using (Font gameOverFont = new Font("Arial", 48, FontStyle.Bold))
                {
                    SizeF textSize = g.MeasureString("Game Over", gameOverFont);
                    g.DrawString("Game Over", gameOverFont, Brushes.Red,
                        (this.Width - textSize.Width) / 2,
                        (this.Height - textSize.Height) / 2 - 50);
                }

                using (Font restartFont = new Font("Arial", 24, FontStyle.Bold))
                {
                    SizeF textSize = g.MeasureString("Press Enter to Restart", restartFont);
                    g.DrawString("Press Enter to Restart", restartFont, Brushes.White,
                        (this.Width - textSize.Width) / 2,
                        (this.Height - textSize.Height) / 2 + 10);
                }
            }

            base.OnPaint(e);
        }

        [STAThread]
        public static void Main()
        {
            Application.Run(new GameForm());
        }
    }

    // Updated Platform class
    public class Platform
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }

        public Platform(int x, int y, int width)
        {
            X = x;
            Y = y;
            Width = width;
        }

        public void Draw(Graphics g)
        {
            // Draw a solid platform (avoid using black)
            Brush platformBrush = new SolidBrush(Color.Brown);
            g.FillRectangle(platformBrush, X, Y, Width, 20); // Increased height for a solid look
            platformBrush.Dispose();

            // Optionally, add a border to the platform
            Pen platformPen = new Pen(Color.SaddleBrown, 2);
            g.DrawRectangle(platformPen, X, Y, Width, 20);
            platformPen.Dispose();
        }
    }

    // Player class with left and right movement
    public class Player
    {
        public int X { get; private set; }
        public float Y { get; private set; }
        private int _width = 20;
        private int _height = 50;
        private float _velocityY = 0;
        private float _gravity = 0.5f;
        private float _velocityX = 0;
        private float _speed = 5f;
        public bool IsOnGround { get; private set; }
        private bool _isJumping = false;
        private float _jumpCharge = 0;
        private const float DefaultMaxJumpCharge = 25f;
        private const float MinJumpCharge = 10f;
        private const float JumpChargeRate = 0.5f;
        private float _maxJumpCharge = DefaultMaxJumpCharge;

        public Player()
        {
            X = 100;
            Y = 500 - _height;
            IsOnGround = true;
        }

        public void StartJump()
        {
            if (IsOnGround)
            {
                _isJumping = true;
                _jumpCharge = 0;
            }
        }

        public void EndJump()
        {
            if (_isJumping)
            {
                if (_jumpCharge < MinJumpCharge)
                {
                    _jumpCharge = MinJumpCharge;
                }
                _velocityY = -_jumpCharge;
                _isJumping = false;
                IsOnGround = false;
            }
        }

        public void Update(List<Platform> platforms, bool moveLeft, bool moveRight)
        {
            // Horizontal movement
            if (moveLeft)
            {
                _velocityX = -_speed;
            }
            else if (moveRight)
            {
                _velocityX = _speed;
            }
            else
            {
                _velocityX = 0;
            }

            X += (int)_velocityX;

            // Keep player within screen bounds
            if (X - _width / 2 < 0)
                X = _width / 2;
            if (X + _width / 2 > 800)
                X = 800 - _width / 2;

            // Jump charging
            if (_isJumping)
            {
                _jumpCharge += JumpChargeRate;
                if (_jumpCharge >= _maxJumpCharge)
                {
                    _jumpCharge = _maxJumpCharge;
                    _velocityY = -_jumpCharge;
                    _isJumping = false;
                    IsOnGround = false;
                }
            }

            // Apply gravity
            _velocityY += _gravity;
            Y += _velocityY;

            // Collision detection with platforms
            IsOnGround = false;
            foreach (var platform in platforms)
            {
                if (_velocityY >= 0 &&
                    Y + _height >= platform.Y && Y + _height <= platform.Y + 20 &&
                    X + _width / 2 >= platform.X && X - _width / 2 <= platform.X + platform.Width)
                {
                    Y = platform.Y - _height;
                    _velocityY = 0;
                    IsOnGround = true;
                    break;
                }
            }

            // Ground level collision
            if (Y + _height >= 500)
            {
                Y = 500 - _height;
                _velocityY = 0;
                IsOnGround = true;
            }
        }

        public void Draw(Graphics g)
        {
            Pen pen = new Pen(Color.Blue, 2);

            // Head
            g.DrawEllipse(pen, X - 5, Y - _height, 10, 10);

            // Body
            g.DrawLine(pen, X, Y - _height + 10, X, Y - _height + 30);

            // Arms
            g.DrawLine(pen, X, Y - _height + 15, X - 10, Y - _height + 25);
            g.DrawLine(pen, X, Y - _height + 15, X + 10, Y - _height + 25);

            // Legs
            g.DrawLine(pen, X, Y - _height + 30, X - 10, Y);
            g.DrawLine(pen, X, Y - _height + 30, X + 10, Y);

            pen.Dispose();
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(X - _width / 2, (int)Y - _height, _width, _height);
        }

        public void ActivatePowerUp()
        {
            _maxJumpCharge = DefaultMaxJumpCharge * 2;
            // Optionally, implement a timer to revert the power-up
        }
    }

    // Alien, Obstacle, PowerUp, Background, Star, and Mountain classes remain unchanged
    // Ensure that none of the elements use the color black for their drawing
    // For example, in the Obstacle class, avoid using black
}

// Other classes (Alien, Obstacle, Platform, PowerUp, Background, Star, Mountain)
// remain the same as in the previous code.

// Ensure that all methods and properties are fully implemented.
public class Alien
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    private int _speed;

    public Alien(int startX, int startY, Random random)
    {
        X = startX;
        Y = startY;
        Width = random.Next(40, 80);
        Height = random.Next(40, 80);
        _speed = 5;
    }

    public void Update()
    {
        X -= _speed;
    }

    public void Draw(Graphics g)
    {
        // Draw the alien body
        g.FillEllipse(Brushes.Green, X, Y, Width, Height);

        // Eyes
        int eyeWidth = Width / 5;
        int eyeHeight = Height / 5;
        int eyeY = Y + Height / 4;

        // Left eye
        g.FillEllipse(Brushes.White, X + Width / 4 - eyeWidth / 2, eyeY, eyeWidth, eyeHeight);
        // Right eye
        g.FillEllipse(Brushes.White, X + 3 * Width / 4 - eyeWidth / 2, eyeY, eyeWidth, eyeHeight);

        // Pupils
        int pupilWidth = eyeWidth / 2;
        int pupilHeight = eyeHeight / 2;
        g.FillEllipse(Brushes.Black, X + Width / 4 - pupilWidth / 2, eyeY + eyeHeight / 4, pupilWidth, pupilHeight);
        g.FillEllipse(Brushes.Black, X + 3 * Width / 4 - pupilWidth / 2, eyeY + eyeHeight / 4, pupilWidth, pupilHeight);

        // Angry Eyebrows
        Pen eyebrowPen = new Pen(Color.Black, 2);
        // Left eyebrow
        g.DrawLine(eyebrowPen,
            X + Width / 4 - eyeWidth,
            eyeY,
            X + Width / 4 + eyeWidth / 2,
            eyeY - eyeHeight);
        // Right eyebrow
        g.DrawLine(eyebrowPen,
            X + 3 * Width / 4 + eyeWidth / 2,
            eyeY - eyeHeight,
            X + 3 * Width / 4 + eyeWidth,
            eyeY);
        eyebrowPen.Dispose();

        // Mouth (angry frown)
        Pen mouthPen = new Pen(Color.Black, 2);
        g.DrawArc(mouthPen, X + Width / 4, Y + 3 * Height / 5, Width / 2, Height / 5, 20, 140);
        mouthPen.Dispose();
    }

    public Rectangle GetBounds()
    {
        return new Rectangle(X, Y, Width, Height);
    }
}

public class Obstacle
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    private int _speed;

    public Obstacle(int startX, int startY, Random random)
    {
        X = startX;
        Y = startY;
        Width = random.Next(20, 40);
        Height = random.Next(20, 40);
        _speed = 5;
    }

    public void Update()
    {
        X -= _speed;
    }

    public void Draw(Graphics g)
    {
        // Draw the obstacle as a small block
        g.FillRectangle(Brushes.Brown, X, Y, Width, Height);
    }

    public Rectangle GetBounds()
    {
        return new Rectangle(X, Y, Width, Height);
    }
}

public class Platform
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }

    public Platform(int x, int y, int width)
    {
        X = x;
        Y = y;
        Width = width;
    }

    public void Draw(Graphics g)
    {
        // Draw a simple platform (girder style)
        g.FillRectangle(Brushes.DarkRed, X, Y, Width, 10);
    }
}

public class PowerUp
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    private int _speed;

    public PowerUp(int startX, int startY)
    {
        X = startX;
        Y = startY;
        Width = 20;
        Height = 20;
        _speed = 5;
    }

    public void Update()
    {
        X -= _speed;
    }

    public void Draw(Graphics g)
    {
        // Draw the power-up as a glowing object
        g.FillEllipse(Brushes.Yellow, X, Y, Width, Height);
    }

    public Rectangle GetBounds()
    {
        return new Rectangle(X, Y, Width, Height);
    }
}

public class Background
{
    private int _width;
    private int _height;
    private List<Star> _stars;
    private List<Mountain> _mountains;
    private int _starSpeed = 1;
    private int _mountainSpeed = 2;
    private Random _random;

    public Background(int width, int height)
    {
        _width = width;
        _height = height;
        _stars = new List<Star>();
        _mountains = new List<Mountain>();
        _random = new Random();

        // Initialize stars
        for (int i = 0; i < 50; i++)
        {
            _stars.Add(new Star(_random.Next(_width), _random.Next(_height / 2), _random.Next(1, 3)));
        }

        // Initialize mountains
        _mountains.Add(new Mountain(0, _height - 150));
        _mountains.Add(new Mountain(_width / 2, _height - 150));
    }

    public void Update()
    {
        // Update stars
        foreach (var star in _stars)
        {
            star.X -= _starSpeed;
            if (star.X < 0)
            {
                star.X = _width;
                star.Y = _random.Next(_height / 2);
            }
        }

        // Update mountains
        foreach (var mountain in _mountains)
        {
            mountain.X -= _mountainSpeed;
        }

        // Add new mountain if needed
        if (_mountains[_mountains.Count - 1].X + _width / 2 <= _width)
        {
            _mountains.Add(new Mountain(_width, _height - 150));
        }

        // Remove off-screen mountains
        if (_mountains[0].X + _width / 2 < 0)
        {
            _mountains.RemoveAt(0);
        }
    }

    public void Draw(Graphics g)
    {
        // Draw stars
        foreach (var star in _stars)
        {
            g.FillEllipse(Brushes.White, star.X, star.Y, star.Size, star.Size);
        }

        // Draw mountains
        foreach (var mountain in _mountains)
        {
            Point[] mountainPoints = {
                    new Point(mountain.X, mountain.Y + 150),
                    new Point(mountain.X + _width / 4, mountain.Y),
                    new Point(mountain.X + _width / 2, mountain.Y + 150)
                };
            g.FillPolygon(Brushes.Gray, mountainPoints);
        }
    }
}

public class Star
{
    public int X;
    public int Y;
    public int Size;

    public Star(int x, int y, int size)
    {
        X = x;
        Y = y;
        Size = size;
    }
}

public class Mountain
{
    public int X;
    public int Y;

    public Mountain(int x, int y)
    {
        X = x;
        Y = y;
    }
}

