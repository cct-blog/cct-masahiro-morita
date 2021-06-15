using System;
using System.Linq;
using System.Net.Http.Json;
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

        private Guid _roomId;

        public Guid RoomId
        {
            get => _roomId;
            set => ValueChangeProcess(ref _roomId, value);
        }
        private readonly Chat.IPresenter _presenter;

        public UserListViewModel UserList { get; set; }

        public ContentCollection<PostViewModel> ThreadPosters { get; } = new ContentCollection<PostViewModel>();

        public PostViewModel MessagePoster { get; }


        public Selectable UserListTabOpened { get; } = new() { IsSelected = false, IsEnabled = true };

        public Selectable ThreadTabOpened { get; } = new() { IsSelected = false, IsEnabled = true };

        public ChatViewModel(Chat.IPresenter presenter, Guid roomId)
        {
            _roomId = roomId;
            _presenter = presenter;
            UserList = new(presenter, roomId);

            MessagePoster = new(_presenter, SendMessage, null);

            ThreadPosters.CollectionChanged += (s, e) => _presenter.Invalidate();
            ThreadPosters.PropertyChanged += (s, e) => _presenter.Invalidate();
        }

        public async Task Refresh(Guid roomId)
        {
            _roomId = roomId;
            UserList = new(_presenter, roomId);
            ThreadPosters.Clear();
            await UserList.OnInitializedAsync();
        }

        public async Task OnInitializedAsync()
        {
            await UserList.OnInitializedAsync();
        }

        public void OnMessagePosted(Message message)
        {
            var thread = ThreadPosters.FirstOrDefault(each => each.ParentMessage.Id == message.Id);
            if (thread != null)
            {
                thread.ParentMessage = message;
            }
            else
            {
                ThreadPosters.Add(new(_presenter, text => SendThreadMessage(message.Id, text), message));
            }
        }

        /// <summary>
        /// メッセージを投稿します。
        /// </summary>
        private async Task SendMessage(string text)
        {
            var user = await _presenter.GetUserAsync();

            var message = new Message()
            {
                Id = Guid.Empty,
                UserEmail = user.Id,
                HandleName = user.Name,
                MessageContext = text,
                RoomId = _roomId
            };

            await _model.SendMessage(message, _presenter.GetHabConnection());
        }

        /// <summary>
        /// スレッドのメッセージを投稿します。
        /// </summary>
        private async Task SendThreadMessage(Guid postId, string text)
        {
            var user = await _presenter.GetUserAsync();

            var message = new ThreadMessage()
            {
                ThreadId = Guid.Empty,
                PostId = postId,
                UserEmail = user.Id,
                HandleName = user.Name,
                MessageContext = text,
                RoomId = _roomId
            };

            await _presenter.GetHttpClient().PostAsJsonAsync("Thread", message);
        }
    }
}
