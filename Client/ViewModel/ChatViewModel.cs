using System;
using System.Threading.Tasks;
using blazorTest.Client.Models;
using blazorTest.Client.Pages;
using blazorTest.Shared.Models;
using Oniqys.Blazor.ViewModel;

namespace blazorTest.Client.ViewModel
{
    public class ChatViewModel : ContentBase
    {
        private readonly ChatModel _model = new();

        private readonly Guid _roomId;

        private readonly Chat.IPresenter _presenter;

        public UserListViewModel UserList { get; }

        public ContentCollection<PostViewModel> Posts { get; } = new ContentCollection<PostViewModel>();

        public Selectable UserListOpened { get; } = new() { IsSelected = false, IsEnabled = true };


        public Clickable MessageSender { get; }

        private string _inputText;
        public string InputText { get => _inputText; set => ObjectChangeProcess(ref _inputText, value); }

        public ChatViewModel(Chat.IPresenter presenter, Guid roomId)
        {
            _roomId = roomId;
            _presenter = presenter;
            UserList = new(presenter, roomId);

            MessageSender = new Clickable
            {
                Command = new Command(async () => await SendMessage())
            };
        }

        public async Task OnInitializedAsync()
        {
            await UserList.OnInitializedAsync();
        }

        private async Task SendMessage()
        {
            var user = await _presenter.GetUserAsync();

            var message = new Message()
            {
                Id = Guid.Empty,
                UserEmail = user.Id,
                HandleName = user.Name,
                MessageContext = InputText,
                RoomId = _roomId
            };

            await _model.SendMessage(message, _presenter.GetHabConnection());
            InputText = string.Empty;
        }
    }
}
