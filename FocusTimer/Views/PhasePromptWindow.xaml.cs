using System.Windows;
using FocusTimer.Services;

namespace FocusTimer.Views;

public partial class PhasePromptWindow : Window
{
    private readonly Action _continueAction;
    private PhaseTransitionEventArgs _transition;

    public PhasePromptWindow(PhaseTransitionEventArgs transition, Action continueAction)
    {
        InitializeComponent();
        _transition = transition;
        _continueAction = continueAction;
        UpdateContent();
    }

    public void UpdateTransition(PhaseTransitionEventArgs transition)
    {
        _transition = transition;
        UpdateContent();
    }

    public void UpdateNextPhaseRunning(bool isRunning)
    {
        if (_transition.IsNextPhaseRunning == isRunning) return;
        _transition = _transition with { IsNextPhaseRunning = isRunning };
        UpdateContent();
    }

    private void UpdateContent()
    {
        var isFocusFinished = _transition.CompletedPhase == TimerPhase.Focus;
        TitleText.Text = isFocusFinished ? "Tijd voor een pauze" : "Pauze voorbij";
        MessageText.Text = isFocusFinished
            ? "Goed gewerkt! Je focusperiode is afgerond."
            : "Je pauze is afgelopen. Tijd om weer te focussen.";

        var phaseName = _transition.NextPhase switch
        {
            TimerPhase.Focus => "Focus",
            TimerPhase.ShortBreak => "Korte pauze",
            _ => "Lange pauze"
        };
        NextPhaseText.Text = _transition.IsNextPhaseRunning
            ? $"{phaseName} loopt al."
            : $"Volgende fase: {phaseName} ({(int)_transition.NextPhaseDuration.TotalMinutes} minuten).";

        ContinueButton.Content = _transition.IsNextPhaseRunning
            ? "Begrepen"
            : _transition.NextPhase == TimerPhase.Focus ? "Focus starten" : "Pauze starten";
    }

    private void Continue_Click(object sender, RoutedEventArgs e)
    {
        _continueAction();
        Close();
    }
}
