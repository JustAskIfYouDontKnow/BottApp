﻿namespace BottApp.Database.Service.Keyboards;

public enum MenuButton
{
    ToVotes,
    ToHelp,
    Hi,
}

public enum MainVoteButton
{
    Back,
    GiveAVote,
    AddCandidate,
    ToMainMenu,
    ToChooseNomination,
    ToHelp,
}

public enum VotesButton
{
    Right,
    Left,
    Back,
    Like,
    ToVotes,
    ToHelp,
}
public enum NominationButton
{
  Biggest,
  Smaller,
  Fastest,
}

public enum HelpButton
{
    ToMainMenu,
    TakeAnswer,
}

public enum AdminButton
{
    Approve,
    Decline,
    DocumentApprove,
    DocumentDecline,
    SendOk,
}