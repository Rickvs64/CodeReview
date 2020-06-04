/// <summary>
/// Used by QuizManager to keep track of the current quiz game's progress and action.
/// </summary>
public enum QuizState
{
    Asleep,
    Initializing,
    ShowingScenario,
    AwaitingAnswer,
    ShowingResult,
    EndingGame
}