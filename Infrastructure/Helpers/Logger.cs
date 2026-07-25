using Infrastructure.Models;

namespace Infrastructure.Helpers;

public static class Logger
{
    private static void _log(string msg) => Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")} | {msg}");
    public static void Debug(string msg) => _log($"DEBUG | {msg}");
    public static void Warn(string msg) => _log($"WARN | {msg}");
    public static void Error(string msg) => _log($"ERROR | {msg}");
    public static void Error(Exception ex) => _log($"ERROR | {ex}");
    private static async Task _logAsync(string msg) => await Task.Run(() =>
    {
        Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")} | {msg}");
    });
    public static async Task DebugAsync(string msg) => await _logAsync($"DEBUG | {msg}");
    public static async Task WarnAsync(string msg) => await _logAsync($"WARN | {msg}");
    public static async Task ErrorAsync(string msg) => await _logAsync($"ERROR | {msg}");
    public static async Task ErrorAsync(Exception ex) => await _logAsync($"ERROR | {ex}");
}

