using Repoverse.Input;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Repoverse
{
    public class AnsiShellInput : IProcessKeyPress
    {
        private static bool useAlternativeColor = false;
        private static Task alternatorTask = Task.Run(() =>
        {
            while (true)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
                useAlternativeColor = !useAlternativeColor;
            }
        });

        private readonly string foregroundColor;
        private readonly string backgroundColor;
        private int cursorPosition;
        private string rawText;

        public AnsiShellInput(string foregroundColor="silver", string backgroundColor="black")
        {
            this.foregroundColor = foregroundColor;
            this.backgroundColor = backgroundColor;
            this.RawText = string.Empty;
            this.cursorPosition = 0;
        }

        public string RawText
        {
            get => this.rawText;
            set
            {
                this.rawText = value;
                this.cursorPosition = this.rawText.Length;
            }
        }

        public string AnsiText
        {
            get
            {
                if (this.cursorPosition == this.RawText.Length)
                {
                    return $"{this.RawText}[{(useAlternativeColor ? this.foregroundColor : this.backgroundColor)}]_[/]";
                }

                var cursorChar = this.RawText[this.cursorPosition];
                var preCursorText = this.RawText.Substring(0, this.cursorPosition);
                var postCursorText = this.RawText.Substring(this.cursorPosition + 1);

                var mainStyle = $"{foregroundColor} on {backgroundColor}";
                var altStyle1 = $"black on silver";
                var altStyle2 = $"black on grey66";

                return useAlternativeColor ?
                    $"[{mainStyle}]{preCursorText}[/][{altStyle1}]{cursorChar}[/][{mainStyle}]{postCursorText}[/]" :
                    $"[{mainStyle}]{preCursorText}[/][{altStyle2}]{cursorChar}[/][{mainStyle}]{postCursorText}[/]";
            }
        }

        public void Clear()
        {
            this.RawText = string.Empty;
            this.cursorPosition = 0;
        }

        public void ProcessKeyPress(ConsoleKeyInfo keyInfo)
        {
            switch(keyInfo.Key)
            {
                case ConsoleKey.Backspace:
                    if (this.RawText.Length > 0 && cursorPosition > 0)
                    {
                        this.rawText = this.rawText.Remove(this.cursorPosition - 1, 1);
                        this.cursorPosition--;
                    }

                    break;

                case ConsoleKey.Delete:
                    if (this.cursorPosition < this.rawText.Length)
                    {
                        this.rawText = this.rawText.Remove(this.cursorPosition, 1);
                    }

                    break;

                case ConsoleKey.LeftArrow:
                    this.cursorPosition--;
                    this.cursorPosition = Math.Max(cursorPosition, 0);
                    break;

                case ConsoleKey.RightArrow:
                    this.cursorPosition++;
                    this.cursorPosition = Math.Min(cursorPosition, this.RawText.Length);
                    break;

                default:
                    var character = keyInfo.KeyChar;
                    if (character != '\0')
                    {
                        this.RawText = this.RawText.Insert(this.cursorPosition++, character.ToString());
                    }

                    break;
            }
        }
    }
}
