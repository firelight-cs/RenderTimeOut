using System;
using System.Threading;
using TimerConsole;
using System.Collections.Generic;

namespace TimerConsole
{
    public class TimerEngine
    {
        private int _timeLeft;
        private Timer _timer; // update console display every second (System.Threading.Timer)
        private ITimerState _state; // timer's current behavior

        private AsciiRender _render = new AsciiRender();

        public TimerEngine(int seconds)
        {
            _timeLeft = seconds;
            _state = new ReadyTimerState(this);
        }
        // deligate their behavior to the current state (it changes depend on state)
        public void Run() => _state.Run();
        public void Pause() => _state.Pause();
        public void Resume() => _state.Resume();
        public void Stop() => _state.Stop();

        public void Reset(int seconds)
        {
            _timeLeft = seconds;
            Console.Clear();
            Console.WriteLine($"Timer reset to: {_timeLeft} seconds");
        }
        
        public void Tick()
        {
            if (_timeLeft > 0)
            {
                _timeLeft--;
                if (_timeLeft % 5 == 0 || _timeLeft == 0) // shows time left every 5 seconds
                {
                    Console.Clear();
                    _render.RenderTime(_timeLeft);
                    /*Console.WriteLine($"Time left: {_timeLeft} seconds");*/
    
                }
                
                if (_timeLeft == 0)
                {
                    Console.WriteLine("Time is up!");
                    Stop();
                }
            }
        }

        public void StartTimer()
        {
            _timer = new Timer(state => Tick(), null, 0, 1000);
        }

        public void StopTimer() => _timer?.Dispose();

        public int TimeLeft => _timeLeft;

        public void SetState(ITimerState newState) => _state = newState;

    }

    public interface ITimerState
    {
        void Run();
        void Pause();
        void Resume();
        void Stop();

    }

    public class ReadyTimerState : ITimerState
    {
        private readonly TimerEngine _timerEngine;
        
        public ReadyTimerState(TimerEngine timerEngine) => _timerEngine = timerEngine;

        public void Run()
        {
            Console.WriteLine("Starting timer...");
            _timerEngine.SetState(new RunningTimerState(_timerEngine));
            _timerEngine.StartTimer();
        }

        public void Pause()
        {
            Console.WriteLine("Cannot pause. Timer is not running.");
        }

        public void Resume() => Console.WriteLine("Timer is not running");

        public void Stop()
        {
            Console.WriteLine("Timer is not running");
        }

    }

    public class RunningTimerState : ITimerState
    {
        private readonly TimerEngine _timerEngine;
        public RunningTimerState(TimerEngine timerEngine) => _timerEngine = timerEngine;

        public void Run() => Console.WriteLine("Timer is already running");

        public void Pause()
        {
            Console.WriteLine("Pausing timer...");
            _timerEngine.StopTimer();
            _timerEngine.SetState(new PausedTimerState(_timerEngine));
        }
        public void Resume() => Console.WriteLine("Timer is already running");

        public void Stop()
        {
            Console.WriteLine("Stopping timer...");
            _timerEngine.StopTimer();
            _timerEngine.SetState(new ReadyTimerState(_timerEngine));
        }
        

    }

    public class PausedTimerState : ITimerState
    {
        private readonly TimerEngine _timerEngine;
        
        public PausedTimerState(TimerEngine timerEngine) => _timerEngine = timerEngine;

        public void Run() => Console.WriteLine("Timer is paused. Use resume to continue");
        
        public void Pause() => Console.WriteLine("Timer is already paused");

        public void Resume()
        {
            Console.WriteLine("Resuming timer...");
            _timerEngine.SetState(new RunningTimerState(_timerEngine));
            _timerEngine.StartTimer();
        }

        public void Stop()
        {
            Console.WriteLine("Stopping timer...");
            _timerEngine.SetState(new ReadyTimerState(_timerEngine));
        }


    }

    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int seconds))
            {
                Console.WriteLine("Usage: timeout [seconds]");
                return;
            }

            var timerEngine = new TimerEngine(seconds);
            timerEngine.Run();
            
            Console.WriteLine("Available commands: pause, resume, stop, reset");

            while (true)
            {
                var input = Console.ReadLine();
                if (input == null) continue;

                switch (input.Trim().ToLower())
                {
                    case "pause":
                        timerEngine.Pause();
                        break;
                    case "resume":
                        timerEngine.Resume();
                        break;
                    case "stop":
                        timerEngine.Stop();
                        break;
                    case "reset":
                        Console.WriteLine("Enter new timeout: ");
                        var newTimeStr = Console.ReadLine();
                        if (int.TryParse(newTimeStr, out int newTime))
                        {
                            timerEngine.Reset(newTime);
                            timerEngine.Run();
                        }
                        else
                        {
                            Console.WriteLine("Invalid time input");
                        }

                        break;
                    
                    default:
                        Console.WriteLine("Commands: pause, resume, stop, reset");
                        break;
                        
                }
            }
        }
        
    }
}

public class AsciiRender
{
    private static readonly Dictionary<char, string[]> BigDigits = new Dictionary<char, string[]>()
    {
        {'0', new[] { "  ____  ", " / __ \\ ", "| |  | |", "| |  | |", "| |__| |", " \\____/ " } },
        {'1', new[] { " __ ", "/_ |", " | |", " | |", " | |", " |_|" } },
        {'2', new[] { " ___  ", "|__ \\ ", "   ) |", "  / / ", " / /_ ", "|____|" } },
        {'3', new[] { " ____  ", "|___ \\ ", "  __) |", " |__ < ", " ___) |", "|____/ " } },
        {'4', new[] { " _  _   ", "| || |  ", "| || |_ ", "|__   _|", "   | |  ", "   |_|  " } },
        {'5', new[] { " _____ ", "| ____|", "| |__  ", "|___ \\ ", " ___) |", "|____/ " } },
        {'6', new[] { "   __  ", "  / /  ", " / /_  ", "| '_ \\ ", "| (_) |", " \\___/ " } },
        {'7', new[] { " ______", "|____  |", "    / / ", "   / /  ", "  / /   ", " /_/    " } },
        {'8', new[] { "  ___  ", " / _ \\ ", "| (_) |", " > _ < ", "| (_) |", " \\___/ " } },
        {'9', new[] { "  ___  ", " / _ \\ ", "| (_) |", " \\__, |", "   / / ", "  /_/  " } },

    };

    public void RenderTime(int seconds)
    {
        string timeString = seconds.ToString();
        int linesCount = BigDigits['0'].Length; // '0' = timeString
        string[] resultLines = new string[linesCount];

        foreach (char c in timeString)
        {
            if (!BigDigits.ContainsKey(c)) continue; // just in case 

            string[] digitArt = BigDigits[c];
            for (int i = 0; i < linesCount; i++)
            {
                resultLines[i] += digitArt[i] + "  ";
            }
        }
        
        Console.Clear();
        foreach (var line in resultLines)
        {
            Console.WriteLine(line);
        }
    }
}