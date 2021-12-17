using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatApp.Client.Models;
using ChatApp.Client.Pages;
using ChatApp.Client.Services;
using ChatApp.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Oniqys.Blazor.ViewModel;

namespace ChatApp.Client.ViewModel
{
    public class UserListViewModel : ContentBase
    {
        private readonly Chat.IPresenter _presenter;

        private readonly ChatModel _chatModel;

        private readonly IndexModel _indexModel;

        private readonly AuthenticationStateProvider _authenticationStateProvider;

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

        public async Task SetRoomId(Guid roomId)
        {
            RoomId = roomId;
            await _chatModel.GetAllRoomParticipantsAsync(roomId);
        }

        private string _userEmail;

        public string UserEmail
        {
            get => _userEmail;
            set
            {
                ObjectChangeProcess(ref _userEmail, value);
            }
        }

        public UserListViewModel(Chat.IPresenter presenter, Guid roomId, IndexModel indexModel, ChatModel chatModel, AuthenticationStateProvider authenticationStateProvider)
        {
            _indexModel = indexModel;
            _presenter = presenter;
            _authenticationStateProvider = authenticationStateProvider;

            if (presenter is null || roomId == Guid.Empty || chatModel == null)
                return;

            _chatModel = chatModel;
            // roomIdの更新によってイベントハンドラーが呼ばれる前に設定する
            _chatModel.RoomParticipantsChanged += (s, e) => RoomParticipantsChanged(e);

            _roomId = roomId;
            Task.Run(async () =>
            {
                var user = await _indexModel.GetUserAsync(_authenticationStateProvider);
                _userEmail = user.Id;
            });
        }

        // 画面のボタンクリックで参照
        public async Task LeaveRoom()
        {
            var user = await _indexModel.GetUserAsync(_authenticationStateProvider);
            await _chatModel.DeleteUsersAsync(new List<string>() { user.Id });

            await _indexModel.GetUserBelongedRoomsAsync(_authenticationStateProvider);

            _presenter.GetNavigationManager().NavigateTo("/");
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

                var isParticipate = (userInformations.FirstOrDefault(each => each.Email == user.Email) is null) ?
                    false : true;

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

            _presenter.Invalidate();
        }
    }
}