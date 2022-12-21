using BottApp.Host.Services.Handlers.AdminChat;
using BottApp.Host.Services.Handlers.Auth;
using BottApp.Host.Services.Handlers.Help;
using BottApp.Host.Services.Handlers.MainMenu;
using BottApp.Host.Services.Handlers.UploadHandler;
using BottApp.Host.Services.Handlers.Votes;

namespace BottApp.Host.Services.Handlers;

public interface IHandlerContainer
{
    IAdminChatHandler AdminChatHandler { get; }
    IAuthHandler AuthHandler { get; }
    IMainMenuHandler MainMenuHandler { get; }
    IVotesHandler VotesHandler { get; }
    ICandidateUploadHandler CandidateUploadHandler { get; }
    IHelpHandler HelpHandler { get; }
}