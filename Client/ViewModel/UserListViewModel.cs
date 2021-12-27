using System;
using System.Collections.Generic;
using System.Linq;
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
    public class UserListViewModel : ContentBase
    {
        private readonly IndexModel _indexModel;

        private readonly AuthenticationStateProvider _authenticationStateProvider;

        private readonly NavigationManager _navigationManager;

        private ChatModel _chatModel;

        private Chat.IPresenter _presenter;

        /// <summary>
        /// 全ユーザーと（入室中情報付き）
        /// </summary>
        public ContentCollection<Selectable<UserInformation>> Users { get; } = new();

        private Guid _roomId;

        /// <summary>
        /// チャットルームID
        /// </summary>
        public Guid RoomId
        {
            get => _roomId;
            private set
            {
                ValueChangeProcess(ref _roomId, value);
            }
        }

        public UserListViewModel(IndexModel indexModel, AuthenticationStateProvider authenticationStateProvider, NavigationManager navigationManager)
        {
            _indexModel = indexModel;
            _authenticationStateProvider = authenticationStateProvider;
            _navigationManager = navigationManager;

            // roomIdの更新によってイベントハンドラーが呼ばれる前に設定する(TODO : 弱参照化)
            Users.PropertyChanged += (s, e) => OnPropertyChanged(nameof(Users));
            Users.CollectionChanged += (s, e) => OnPropertyChanged(nameof(Users));
        }

        public async Task InitializeAsync(Chat.IPresenter presenter, ChatModel chatModel)
        {
            _presenter = presenter;
            _chatModel = chatModel;
            _roomId = chatModel.RoomId;
            // roomIdの更新によってイベントハンドラーが呼ばれる前に設定する(TODO : 弱参照化)
            _chatModel.RoomParticipantsChanged += (s, e) => RoomParticipantsChanged(e);
            await UpdateUsers();
        }

        // 画面のボタンクリックで参照
        public async Task LeaveRoom()
        {
            var user = await _indexModel.GetUserAsync(_authenticationStateProvider);
            await _chatModel.DeleteUsersAsync(new List<string>() { user.Id });

            await _indexModel.GetUserBelongedRoomsAsync(_authenticationStateProvider);

            _navigationManager.NavigateTo("/");
        }

        // 画面のボタンクリックで参照
        public async Task AddUsers()
            => await _chatModel.AddUsersAsync(Users
                .Where(each => each.IsEnabled && each.IsSelected)
                .Select(each => each.Content.Email)
                .ToList());

        public async Task UpdateUsers()
        {
            await _indexModel.GetAllUserAsync();
            await _chatModel.GetAllRoomParticipantsAsync(_roomId);
        }

        public void RoomParticipantsChanged(UserInformation[] userInformations)
        {
            foreach (var user in _indexModel.AllUsers)
            {
                var matchedUser = Users.FirstOrDefault(each => each.Content.Email == user.Email);

                var isParticipate = userInformations.FirstOrDefault(each => each.Email == user.Email) is not null;

                if (matchedUser is null)
                {
                    Users.Add(new Selectable<UserInformation>
                    {
                        IsSelected = isParticipate,
                        IsEnabled = !isParticipate,
                        Content = user
                    });
                }
                else if (matchedUser.IsSelected != isParticipate)
                {
                    matchedUser.IsSelected = isParticipate;
                    matchedUser.IsEnabled = !isParticipate;
                }
            }
            OnPropertyChanged(null);
        }
    }
}