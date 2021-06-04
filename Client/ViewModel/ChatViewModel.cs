using blazorTest.Client.Models;
using blazorTest.Client.Pages;
using blazorTest.Shared.Models;
using Oniqys.Blazor.ViewModel;
using System;
using System.Threading.Tasks;

namespace blazorTest.Client.ViewModel
{
    public class ChatViewModel : ContentBase
    {
        private readonly ChatModel _model = new();

        private readonly Guid _roomId;

        private readonly Chat.IPresenter _presenter;

        /// <summary>
        /// 全ユーザーと（入室中情報付き）
        /// </summary>
        public ContentCollection<Selectable<UserInformation>> Users { get; set; } = new();

        public ContentCollection<PostViewModel> Posts { get; } = new ContentCollection<PostViewModel>();

        public Selectable UserListOpened { get; } = new() { IsSelected = false, IsEnabled = true };


        public Clickable MessageSender { get; }

        private string _inputText;
        public string InputText { get => _inputText; set => ObjectChangeProcess(ref _inputText, value); }

        public ChatViewModel(Chat.IPresenter presenter, Guid roomId)
        {
            _roomId = roomId;
            _presenter = presenter;

            MessageSender = new Clickable
            {
                Command = new Command(async () => await SendMessage())
            };
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
