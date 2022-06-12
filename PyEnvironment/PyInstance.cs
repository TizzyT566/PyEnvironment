using System.Diagnostics;

namespace PyEnvironment
{
    public class PyInstance
    {
        private readonly Process _python;

        public PyInstance(Action<string> output, Action<string>? error = null, int ignoreInitialLines = 2)
        {
            if (output is null) throw new ArgumentNullException(nameof(output));

            _python = new();
            _python.StartInfo.FileName = "python";
            _python.StartInfo.Arguments = "-i";
            _python.StartInfo.RedirectStandardInput = true;
            _python.StartInfo.RedirectStandardOutput = true;
            _python.StartInfo.RedirectStandardError = true;
            _python.StartInfo.UseShellExecute = false;

            int outputLines = ignoreInitialLines;
            int errorLines = ignoreInitialLines;

            _python.OutputDataReceived += (sender, e) =>
            {
                if (outputLines > 0) outputLines--;
                else if (e.Data is not null && e.Data.Trim() != "" && e.Data.Trim() != "undefined")
                {
                    if (e.Data.StartsWith(">>> "))
                        output.Invoke(e.Data[4..]);
                    else
                        output.Invoke(e.Data);
                }
            };
            _python.ErrorDataReceived += (sender, e) =>
            {
                if (errorLines > 0) errorLines--;
                else if (error is not null && e.Data is not null && e.Data.Trim() != "" && e.Data.Trim() != "undefined")
                {
                    if (e.Data.StartsWith(">>> "))
                        error.Invoke(e.Data[4..]);
                    else
                        error.Invoke(e.Data);
                }
            };
            _python.Start();
            _python.BeginOutputReadLine();
            _python.BeginErrorReadLine();
        }

        public Task WaitForExitAsync() => _python.WaitForExitAsync();

        public Task WriteAsync(string content) => _python.StandardInput.WriteAsync(content);
    }
}
