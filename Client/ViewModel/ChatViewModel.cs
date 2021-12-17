using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatApp.Client.Models;
using ChatApp.Client.Pages;
using ChatApp.Client.Services;
using ChatApp.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Oniqys.Blazor.ViewModel;

namespace ChatApp.Client.ViewModel
{
    public class ChatViewModel : ContentBase
    {
        private readonly ChatModel _chatModel;

        private readonly IndexModel _indexModel;

        private readonly HubUtility _hubUtility;

        private Guid _roomId;

        private Task _updating = Task.CompletedTask;

        public Guid RoomId
        {
            get => _roomId;
            set
            {
                _updating = _chatModel.UpdateRoomModelAsync(value);
                ValueChangeProcess(ref _roomId, value);
            }
        }

        private string _userEmail;

        public string UserEmail
        {
            get => _userEmail;
            set => ObjectChangeProcess(ref _userEmail, value);
        }

        private string _handleName;

        public string HandleName
        {
            get => _handleName;
            set => ObjectChangeProcess(ref _handleName, value);
        }

        private Chat.IPresenter _presenter;

        public UserListViewModel UserList { get; set; }

        /// <summary>
        /// スレッドにメッセージを投稿するためのViewModel
        /// </summary>
        public ContentCollection<PostViewModel> ThreadPosters { get; } = new ContentCollection<PostViewModel>();

        private PostViewModel _messagePoster;
        /// <summary>
        /// 親メッセージを投稿するためのViewModel
        /// </summary>
        public PostViewModel MessagePoster
        {
            get => _messagePoster;
            private set => ObjectChangeProcess(ref _messagePoster, value);
        }

        public Selectable UserListTabOpened { get; } = new() { IsSelected = false, IsEnabled = true };

        public Selectable ThreadTabOpened { get; } = new() { IsSelected = false, IsEnabled = true };

        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public ChatViewModel(IndexModel indexModel, HubUtility hubUtility, AuthenticationStateProvider authenticationStateProvider, UserListViewModel userListViewModel)
        {
            _hubUtility = hubUtility;
            _indexModel = indexModel;
            _authenticationStateProvider = authenticationStateProvider;
            _chatModel = _indexModel.ChatModelFactory(_hubUtility);
            UserList = userListViewModel;
            MessagePoster = new(null, SendMessage, null);

            // コレクションの変更を画面に伝える
            ThreadPosters.CollectionChanged += (s, e) => OnPropertyChanged(nameof(ThreadPosters));
            ThreadPosters.PropertyChanged += (s, e) => OnPropertyChanged(nameof(ThreadPosters));

            _chatModel.PostsChanged += (s, e) => OnPostChanged(e);
            _chatModel.ThreadsChanged += (s, e) => OnThreadChanged(e);
        }

        public async Task InitializeAsync(Chat.IPresenter presenter, Guid roomId)
        {
            _roomId = roomId;
            _presenter = presenter;
            MessagePoster = new(_presenter, SendMessage, null);

            await _chatModel.Initialize(roomId);
            await UserList.InitializeAsync(_presenter, _chatModel);

            var user = await _indexModel.GetUserAsync(_authenticationStateProvider);
            _userEmail = user.Id;
            _handleName = user.Name;

        }

        /// <summary>
        /// メッセージを投稿します。
        /// </summary>
        private async Task SendMessage(string text)
        {
            var user = await _indexModel.GetUserAsync(_authenticationStateProvider);

            var message = new Message()
            {
                Id = Guid.Empty,
                UserEmail = user.Id,
                HandleName = user.Name,
                MessageContext = text,
                RoomId = _roomId
            };

            await _chatModel.SendMessageAsync(message);
        }

        /// <summary>
        /// スレッドのメッセージを投稿します。
        /// </summary>
        private async Task SendThreadMessage(Guid postId, string text)
        {
            var user = await _indexModel.GetUserAsync(_authenticationStateProvider);

            var message = new ThreadMessage()
            {
                ThreadId = Guid.Empty,
                PostId = postId,
                UserEmail = user.Id,
                HandleName = user.Name,
                MessageContext = text,
                RoomId = _roomId
            };

            await _chatModel.SendThreadMessageAsync(message);
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
            OnPropertyChanged(null);
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
            => await _chatModel.GetRoomPostsAsync();

        public string NextFocusElementId { get; set; }

        private bool _isOpenedUserList;

        public bool IsOpenedUserList
        {
            get => _isOpenedUserList;
            set
            {
                if (_isOpenedUserList == value)
                    return;

                _currentThreadId = Guid.Empty;
                ValueChangeProcess(ref _isOpenedUserList, value);
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

                OnPropertyChanged();
            }
        }

        public PostViewModel CurrentThread { get; set; } = new(null, null, null);

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
            await _chatModel.GetThreadMessageAsync(thread.ParentMessage.Id);
        }

        public async Task OpenInnerThread(PostViewModel poster)
        {
            poster.ThreadOpend = !poster.ThreadOpend;
            await UpdateThread(poster);

            OnPropertyChanged(null);
        }

        public void OpenUserList()
        {
            IsOpenedUserList = !IsOpenedUserList;
        }
    }
}