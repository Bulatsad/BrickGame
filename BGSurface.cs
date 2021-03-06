﻿using System;
using System.Media;
using System.Threading;

namespace BrickGameEmulator
{
    public class BGSurface : BGConstants
    {
        private readonly BGDataStorage storage;
        
        private readonly int surfacePositionX;
        private readonly int surfacePositionY;

        public string nameOfGame;

        public int Score { get; set; } = GAME_SCORE;
        public int HighScore { get; set; } = GAME_HIGHSCORE;
        public int Level { get; set; } = GAME_LEVEL;
        public int Speed { get; set; } = GAME_SPEED;

        private int splashPosition;
        private int splashTimeOut = 25;

        private BGField[] splashFrames;

        private SoundPlayer player;

        private bool splashIsPlaying = false;

        public bool SplashIsPlaying => splashIsPlaying;        

        public BGSurface(int surfacePositionX, int surfacePositionY, BGDataStorage storage)
        {
            this.storage = storage;
            this.surfacePositionX = surfacePositionX;
            this.surfacePositionY = surfacePositionY;
            
            Console.CursorVisible = false;
            Console.Title = "BrickGame";
            
            _drawBorder(surfacePositionX, surfacePositionY);
            
            PrintMessageAtPosition(27, 1, "HI-SCORE", ConsoleColor.White);
            PrintMessageAtPosition(27, 4, "SCORE", ConsoleColor.White);
            PrintMessageAtPosition(27, 7, "LEVEL", ConsoleColor.White);
            PrintMessageAtPosition(27, 10, "SPEED", ConsoleColor.White);
            PrintMessageAtPosition(27, 15, "<--CONTROLS-->", ConsoleColor.Red);
            PrintMessageAtPosition(27, 16, "P - Pause/Start", ConsoleColor.Green);
            PrintMessageAtPosition(27, 17, "Pg Up - Next Game", ConsoleColor.Green);
            PrintMessageAtPosition(27, 18, "Pg Dn - Previous Game", ConsoleColor.Green);
            PrintMessageAtPosition(27, 19, "R - Reset", ConsoleColor.Green);
            
            _renderStatusPanel();
        }

        public void InitGame(Game game)
        {
            nameOfGame = game.ToString();
            HighScore = storage.GetInt(nameOfGame, 0);
            Score = 0;
        }
        
        public void Render(BGField bgField)
        {
            Console.BackgroundColor = ConsoleColor.White;
            if (splashIsPlaying || bgField == null)
            {
                _showSplash();
            }
            else
            {
                _render(bgField);
                _renderStatusPanel();
            }
        }

        public void _renderStatusPanel()
        {
            _updateHighScore();
            _updateScore();
            _updateLevel();
            _updateSpeed();
        }

        private void _updateHighScore()
        {
            if (Score > HighScore)
            {
                HighScore = Score;
                storage.PutInt(nameOfGame, HighScore);
            }
            PrintMessageAtPosition(27, 2, HighScore.ToString(), ConsoleColor.White);
        }

        private void _updateScore()
        {
            PrintMessageAtPosition(27, 5, Score.ToString(), ConsoleColor.White);
        }

        private void _updateLevel()
        {
            PrintMessageAtPosition(27, 8, Level.ToString(), ConsoleColor.White);
        }

        private void _updateSpeed()
        {
            PrintMessageAtPosition(27, 11, Speed.ToString(), ConsoleColor.White);
        }
        
        private void _render(BGField bgField)
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    if (bgField.GetValueByPosition(i, j) == 0)
                    {
                        PrintAtPosition(surfacePositionX + 2 + (i * 2), j + 1, "  ", ConsoleColor.White);
                    }
                    else
                    {
                        PrintAtPosition(surfacePositionX + 2 + (i * 2), j + 1, "▣ ", ConsoleColor.Black);
                    }   
                }
            }
        }

        private void _drawBorder(int x, int y)
        {
            for (int i = x; i < x + FIELD_WIDTH * 2 + 4; i++)
            {
                for (int j = y; j < y + FIELD_HEIGHT + 2; j++)
                {
                    if (i == x + 1 && j == y)
                    {
                        PrintAtPosition(i, j, '\u2554', ConsoleColor.White);
                    }
                    else if ((i == x + 1 || i == x + FIELD_WIDTH * 2 + 2) && j != FIELD_HEIGHT + 1 && j != y)
                    {
                        PrintAtPosition(i, j, '\u2551', ConsoleColor.White);
                    }
                    else if (i == x + 1 && j == FIELD_HEIGHT + 1)
                    {
                        PrintAtPosition(i, j, '\u255A', ConsoleColor.White);
                    }
                    else if (i == x + FIELD_WIDTH * 2 + 2 && j == y)
                    {
                        PrintAtPosition(i, j, '\u2557', ConsoleColor.White);
                    }
                    else if (i == x + FIELD_WIDTH * 2 + 2 && j == FIELD_HEIGHT + 1)
                    {
                        PrintAtPosition(i, j, '\u255D', ConsoleColor.White);
                    }
                    else if (i != x && i != x + 1 && i != x + FIELD_WIDTH * 2 + 2 && i != x + FIELD_WIDTH * 2 + 3 && (j == y || j == FIELD_HEIGHT + 1))
                    {
                        PrintAtPosition(i, j, '\u2550', ConsoleColor.White);
                    }
                }
            }
        }

        public void SetSplash(string filename)
        {
            SetSplash(filename, 20);
        }

        public void SetSplash(string filename, int timeout)
        {
            splashTimeOut = timeout;
            splashFrames = new SplashReader().Read(filename);
            StartSplash();
        }

        public void StartSplash()
        {
            splashIsPlaying = true;
            splashPosition = 0;
        }

        public void StopSplash()
        {
            splashIsPlaying = false;
            splashPosition = 0;
        }

        private void _showSplash()
        {
            if (splashFrames == null || splashPosition >= splashFrames.Length || !splashIsPlaying)
            {
                StopSplash();
                return;
            }
            
            _render(splashFrames[splashPosition]);
            Thread.Sleep(splashTimeOut);
            splashPosition++;
        }

        public void Pause(bool pause)
        {
            PrintMessageAtPosition(27, 13, pause ? "PAUSE" : "     ", ConsoleColor.Yellow);
        }

        public void PlaySound(string filename)
        {
            player = new SoundPlayer(filename);
            player.Play();
        }

        public void StopSound()
        {
            player.Stop();
        }

        public void PrintAtPosition(int x, int y, char symbol, ConsoleColor color)
        {
            Console.SetCursorPosition(x, y);
            Console.BackgroundColor = color;
            Console.Write(symbol);
            Console.BackgroundColor = ConsoleColor.White;
        }
        
        private void PrintAtPosition(int x, int y, string symbol, ConsoleColor color)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = color;
            Console.Write(symbol);
            Console.BackgroundColor = ConsoleColor.White;
        }

        public void PrintMessageAtPosition(int x, int y, string text, ConsoleColor color)
        {
            Console.SetCursorPosition(x, y);
            Console.ResetColor();
            Console.ForegroundColor = color;
            Console.Write("            ");
            Console.SetCursorPosition(x, y);
            Console.WriteLine(text);
        }
    }
}