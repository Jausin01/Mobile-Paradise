using System;

namespace MobileParadise.Games.Breakout.Logic
{
    [Flags]
    public enum BreakoutEvents
    {
        None = 0,
        Wall = 1,
        Paddle = 2,
        Brick = 4,          // a brick was hit
        BrickBroken = 8,    // ... and it is gone
        LifeLost = 16,
        LevelCleared = 32,
        GameOver = 64
    }

    /// <summary>
    /// The rules and physics of Breakout. No Unity code here.
    /// The field is 1000 wide and 1400 tall; (0, 0) is the top-left corner and y grows downward, like the screen.
    /// Step(dt) moves the ball; call it with small time steps (the view uses 1/120 s).
    /// </summary>
    public class BreakoutWorld
    {
        public const float FieldWidth = 1000f;
        public const float FieldHeight = 1400f;
        public const float BrickWidth = FieldWidth / BreakoutLevels.Columns;
        public const float BrickHeight = 50f;
        public const float BrickTop = 150f;
        public const float PaddleTop = 1300f;
        public const float PaddleHeight = 28f;
        public const float BallRadius = 17f;

        private const float MaxBounceAngle = 1.1f;      // radians from straight up, at the paddle's edge
        private const float MinVerticalShare = 0.25f;   // the ball never gets (nearly) horizontal
        private const float MaxMoveStep = 8f;           // the ball never moves more than this in one sub-step

        private readonly BreakoutDifficulty _difficulty;
        private readonly Random _random;
        private readonly int[] _bricks;

        private float _paddleX;
        private float _paddleWidth;
        private float _ballX;
        private float _ballY;
        private float _velocityX;
        private float _velocityY;
        private float _speed;
        private bool _launched;
        private int _bricksLeft;
        private int _level;
        private int _score;
        private int _lives;

        public BreakoutWorld(BreakoutDifficulty difficulty, int level, int score, int lives, Random random)
        {
            _difficulty = difficulty;
            _random = random;
            _level = level;
            _score = score;
            _lives = lives;

            _paddleWidth = BreakoutSettings.PaddleWidth(difficulty);
            _paddleX = FieldWidth / 2f;
            _speed = BreakoutSettings.BallSpeed(difficulty) * Math.Min(1.6f, 1f + 0.05f * (level - 1));

            _bricks = BreakoutLevels.Bricks(level);
            for (int i = 0; i < _bricks.Length; i++)
            {
                if (_bricks[i] > 0) _bricksLeft++;
            }

            PutBallOnPaddle();
        }

        public BreakoutDifficulty Difficulty { get { return _difficulty; } }
        public int Level { get { return _level; } }
        public int Score { get { return _score; } }
        public int Lives { get { return _lives; } }
        public int BricksLeft { get { return _bricksLeft; } }
        public bool Launched { get { return _launched; } }
        public float PaddleX { get { return _paddleX; } }
        public float PaddleWidth { get { return _paddleWidth; } }
        public float BallX { get { return _ballX; } }
        public float BallY { get { return _ballY; } }

        public int BrickStrength(int index)
        {
            return _bricks[index];
        }

        /// <summary>
        /// Moves the paddle (its center) and keeps it inside the field. Before the launch the ball goes along.
        /// </summary>
        public void MovePaddleTo(float x)
        {
            float half = _paddleWidth / 2f;
            _paddleX = Math.Max(half, Math.Min(FieldWidth - half, x));
            if (!_launched) PutBallOnPaddle();
        }

        /// <summary>
        /// Sends the ball up, with a small random slant.
        /// </summary>
        public bool Launch()
        {
            if (_launched || _lives <= 0) return false;

            float angle = ((float)_random.NextDouble() * 0.7f - 0.35f);
            _velocityX = _speed * (float)Math.Sin(angle);
            _velocityY = -_speed * (float)Math.Cos(angle);
            _launched = true;
            return true;
        }

        /// <summary>
        /// Moves the ball for 'dt' seconds and returns what happened.
        /// </summary>
        public BreakoutEvents Step(float dt)
        {
            BreakoutEvents events = BreakoutEvents.None;
            if (!_launched || _lives <= 0 || _bricksLeft == 0) return events;

            float distance = _speed * dt;
            int parts = Math.Max(1, (int)Math.Ceiling(distance / MaxMoveStep));
            float partTime = dt / parts;

            for (int i = 0; i < parts; i++)
            {
                _ballX += _velocityX * partTime;
                _ballY += _velocityY * partTime;

                events |= BounceOnWalls();
                events |= BounceOnPaddle();
                events |= BounceOnBricks();

                if (_bricksLeft == 0)
                {
                    _launched = false;
                    return events | BreakoutEvents.LevelCleared;
                }

                if (_ballY - BallRadius > FieldHeight)
                {
                    _lives--;
                    _launched = false;
                    PutBallOnPaddle();
                    events |= BreakoutEvents.LifeLost;
                    if (_lives <= 0) events |= BreakoutEvents.GameOver;
                    return events;
                }
            }
            return events;
        }

        // ---- Collisions -------------------------------------------------------

        private BreakoutEvents BounceOnWalls()
        {
            BreakoutEvents events = BreakoutEvents.None;

            if (_ballX - BallRadius < 0f)
            {
                _ballX = BallRadius;
                _velocityX = Math.Abs(_velocityX);
                events = BreakoutEvents.Wall;
            }
            else if (_ballX + BallRadius > FieldWidth)
            {
                _ballX = FieldWidth - BallRadius;
                _velocityX = -Math.Abs(_velocityX);
                events = BreakoutEvents.Wall;
            }

            if (_ballY - BallRadius < 0f)
            {
                _ballY = BallRadius;
                _velocityY = Math.Abs(_velocityY);
                events = BreakoutEvents.Wall;
            }

            if (events != BreakoutEvents.None) KeepMovingUp();
            return events;
        }

        private BreakoutEvents BounceOnPaddle()
        {
            if (_velocityY <= 0f) return BreakoutEvents.None;

            float half = _paddleWidth / 2f;
            bool overlapsY = _ballY + BallRadius >= PaddleTop && _ballY < PaddleTop + PaddleHeight;
            bool overlapsX = _ballX >= _paddleX - half - BallRadius * 0.6f && _ballX <= _paddleX + half + BallRadius * 0.6f;
            if (!overlapsY || !overlapsX) return BreakoutEvents.None;

            // Where the ball hits decides the angle: the middle goes straight up, the edges go sideways.
            float t = Math.Max(-1f, Math.Min(1f, (_ballX - _paddleX) / half));
            float angle = t * MaxBounceAngle;
            _velocityX = _speed * (float)Math.Sin(angle);
            _velocityY = -_speed * (float)Math.Cos(angle);
            _ballY = PaddleTop - BallRadius;
            return BreakoutEvents.Paddle;
        }

        private BreakoutEvents BounceOnBricks()
        {
            int firstColumn = Math.Max(0, (int)((_ballX - BallRadius) / BrickWidth));
            int lastColumn = Math.Min(BreakoutLevels.Columns - 1, (int)((_ballX + BallRadius) / BrickWidth));
            int firstRow = Math.Max(0, (int)((_ballY - BallRadius - BrickTop) / BrickHeight));
            int lastRow = Math.Min(BreakoutLevels.Rows - 1, (int)((_ballY + BallRadius - BrickTop) / BrickHeight));
            if (_ballY + BallRadius < BrickTop) return BreakoutEvents.None;

            for (int row = firstRow; row <= lastRow; row++)
            {
                for (int column = firstColumn; column <= lastColumn; column++)
                {
                    int index = row * BreakoutLevels.Columns + column;
                    if (_bricks[index] <= 0) continue;

                    float left = column * BrickWidth;
                    float top = BrickTop + row * BrickHeight;
                    float nearestX = Math.Max(left, Math.Min(left + BrickWidth, _ballX));
                    float nearestY = Math.Max(top, Math.Min(top + BrickHeight, _ballY));
                    float dx = _ballX - nearestX;
                    float dy = _ballY - nearestY;
                    if (dx * dx + dy * dy >= BallRadius * BallRadius) continue;

                    ReflectFromBrick(dx, dy, nearestX, nearestY);
                    return HitBrick(index);
                }
            }
            return BreakoutEvents.None;
        }

        private void ReflectFromBrick(float dx, float dy, float nearestX, float nearestY)
        {
            if (dx == 0f && dy == 0f)
            {
                _velocityY = -_velocityY;   // the center is inside the brick: just turn around
            }
            else if (Math.Abs(dx) > Math.Abs(dy))
            {
                float side = dx > 0f ? 1f : -1f;
                _velocityX = side * Math.Abs(_velocityX);
                _ballX = nearestX + side * BallRadius;
            }
            else
            {
                float side = dy > 0f ? 1f : -1f;
                _velocityY = side * Math.Abs(_velocityY);
                _ballY = nearestY + side * BallRadius;
            }
            KeepMovingUp();
        }

        private BreakoutEvents HitBrick(int index)
        {
            _bricks[index]--;
            if (_bricks[index] > 0)
            {
                _score += 5;
                return BreakoutEvents.Brick;
            }

            _bricksLeft--;
            _score += 10;
            return BreakoutEvents.Brick | BreakoutEvents.BrickBroken;
        }

        // ---- Helpers ----------------------------------------------------------

        // Keeps the speed constant and the ball from going (nearly) horizontal, which could last forever.
        private void KeepMovingUp()
        {
            float minVertical = _speed * MinVerticalShare;
            if (Math.Abs(_velocityY) < minVertical)
            {
                _velocityY = (_velocityY < 0f ? -1f : 1f) * minVertical;
            }

            float length = (float)Math.Sqrt(_velocityX * _velocityX + _velocityY * _velocityY);
            _velocityX *= _speed / length;
            _velocityY *= _speed / length;
        }

        private void PutBallOnPaddle()
        {
            _ballX = _paddleX;
            _ballY = PaddleTop - BallRadius;
            _velocityX = 0f;
            _velocityY = 0f;
        }
    }
}
