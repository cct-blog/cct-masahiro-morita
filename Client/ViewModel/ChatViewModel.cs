using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatApp.Client.Models;
using ChatApp.Client.Pages;
using ChatApp.Shared.Models;
using Oniqys.Blazor.ViewModel;

namespace ChatApp.Client.ViewModel
{
    public class ChatViewModel : ContentBase
    {
        private readonly ChatModel _model;

        private readonly IndexViewModel _indexViewModel;

        private Guid _roomId;

        public Guid RoomId
        {
            get => _roomId;
            set
            {
                Task.Run(async () => await _model.UpdateRoomModelAsync(value));
                UserList.RoomId = value;
                ValueChangeProcess(ref _roomId, value);
            }
        }

        private string _userEmail;

        public string UserEmail
        {
            get => _userEmail;
            set
            {
                _userEmail = value;
                _presenter.Invalidate();
            }
        }

        private string _handleName;

        public string HandleName
        {
            get => _handleName;
            set
            {
                _handleName = value;
                _presenter.Invalidate();
            }
        }

        private readonly Chat.IPresenter _presenter;

        public UserListViewModel UserList { get; set; }

        /// <summary>
        /// スレッドにメッセージを投稿するためのViewModel
        /// </summary>
        public ContentCollection<PostViewModel> ThreadPosters { get; } = new ContentCollection<PostViewModel>();

        /// <summary>
        /// 親メッセージを投稿するためのViewModel
        /// </summary>
        public PostViewModel MessagePoster { get; }

        public Selectable UserListTabOpened { get; } = new() { IsSelected = false, IsEnabled = true };

        public Selectable ThreadTabOpened { get; } = new() { IsSelected = false, IsEnabled = true };

        public ChatViewModel(Chat.IPresenter presenter, Guid roomId)
        {
            _roomId = roomId;
            _presenter = presenter;
            _model = new ChatModel(_presenter.GetHttpClientFactory(), _presenter.GetHabConnection(), roomId);
            _indexViewModel = presenter.GetIndexViewModel();

            UserList = new(presenter, roomId, _model);

            var user = _presenter.GetUserAsync().Result;
            _userEmail = user.Id;
            _handleName = user.Name;

            MessagePoster = new(_presenter, SendMessage, null);

            ThreadPosters.CollectionChanged += (s, e) => _presenter.Invalidate();
            ThreadPosters.PropertyChanged += (s, e) => _presenter.Invalidate();

            _model.PostsChanged += (s, e) => OnPostChanged(e);
            _model.ThreadsChanged += (s, e) => OnThreadChanged(e);
        }

        public async Task Refresh(Guid roomId)
        {
            _roomId = roomId;
            UserList = new(_presenter, roomId, _model);
            ThreadPosters.Clear();
            await UserList.UpdateUsers();
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

            await _model.SendMessageAsync(message);
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

            await _model.SendThreadMessageAsync(message);
        }

        /// <summary>
        /// Modelのメッセージが変更された場合にPostViewModelを書き換えます
        /// </summary>
        /// <param name="postModels"></param>
        private void OnPostChanged(PostModel[] postModels)
        {
            foreach (var post in postModels)
            {
                var message = new Message
                {
                    Id = post.PostId,
                    RoomId = _roomId,
                    UserEmail = post.UserEmail,
                    HandleName = post.HandleName,
                    MessageContext = post.MessageContext,
                    CreateDate = post.CreateDate,
                    UpdateDate = post.UpdateDate
                };

                OnMessagePosted(message);
            }

            _presenter.Invalidate();
        }

        private void OnMessagePosted(Message message)
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

        private void OnThreadChanged(ThreadModel[] threadModels)
        {
            if (threadModels is null) return;

            foreach (var threadModel in threadModels)
            {
                var message = new Message
                {
                    Id = threadModel.ThreadId,
                    RoomId = _roomId,
                    UserEmail = threadModel.UserEmail,
                    HandleName = threadModel.HandleName,
                    MessageContext = threadModel.MessageContext,
                    CreateDate = threadModel.CreateDate,
                    UpdateDate = threadModel.UpdateDate
                };

                var thread = CurrentThread.Messages.FirstOrDefault(each => each.Id == threadModel.ThreadId);
                if (thread != null)
                {
                    thread = message;
                }
                else
                {
                    CurrentThread.Messages.Add(message);
                }
            }
        }

        /// <summary>
        /// メッセージを更新します。
        /// </summary>
        public async Task RefreshAsync()
            => await _model.GetRoomPostsAsync();

        public string NextFocusElementId { get; set; }

        private bool _isOpenedUserList;

        public bool IsOpenedUserList
        {
            get => _isOpenedUserList;
            set
            {
                if (_isOpenedUserList == value)
                    return;

                _isOpenedUserList = value;
                _currentThreadId = Guid.Empty;
                _presenter.Invalidate();
            }
        }

        private Guid _currentThreadId;

        public Guid CurrentThreadId
        {
            get => _currentThreadId;
            set
            {
                if (_currentThreadId == value)
                    return;

                _isOpenedUserList = false;
                _currentThreadId = value;
                CurrentThread = ThreadPosters
                    .FirstOrDefault(each => each.ParentMessage.Id == CurrentThreadId);
                _presenter.Invalidate();
            }
        }

        public PostViewModel CurrentThread { get; set; }

        public async Task OpenThread(Guid id)
        {
            if (CurrentThreadId == id)
            {
                CurrentThreadId = Guid.Empty;
                return;
            }
            else
            {
                CurrentThreadId = id;
                await UpdateThread(CurrentThread);
            }
        }

        public async Task UpdateThread(PostViewModel thread)
        {
            await _model.GetThreadMessageAsync(thread.ParentMessage.Id);
        }

        public async Task OpenInnerThread(PostViewModel poster)
        {
            poster.ThreadOpend = !poster.ThreadOpend;
            await UpdateThread(poster);

            _presenter.Invalidate();
        }

        public void OpenUserList()
        {
            IsOpenedUserList = !IsOpenedUserList;
        }
    }
}