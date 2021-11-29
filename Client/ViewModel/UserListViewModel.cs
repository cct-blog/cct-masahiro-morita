using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatApp.Client.Models;
using ChatApp.Client.Pages;
using ChatApp.Client.Services;
using ChatApp.Shared.Models;
using Oniqys.Blazor.ViewModel;

namespace ChatApp.Client.ViewModel
{
    public class UserListViewModel : ContentBase
    {
        private readonly Chat.IPresenter _presenter;

        private readonly ChatModel _model;

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
            set
            {
                Task.Run(async () => await _model.GetAllRoomParticipantsAsync());
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

        public UserListViewModel(Chat.IPresenter presenter, Guid roomId, ChatModel model)
        {
            _presenter = presenter;

            _model = model;
            // roomIdの更新によってイベントハンドラーが呼ばれる前に設定する
            _model.RoomParticipantsChanged += (s, e) => RoomParticipantsChanged(e);

            _roomId = roomId;
            _userEmail = _presenter.GetUserAsync().Result.Id;
        }

        // 画面のボタンクリックで参照
        public async Task LeaveRoom()
        {
            var user = await _presenter.GetUserAsync();
            await _model.DeleteUsersAsync(new List<string>() { user.Id });

            await _presenter.GetIndexViewModel().UpdateRoomList();

            _presenter.GetNavigationManager().NavigateTo("/");
        }

        // 画面のボタンクリックで参照
        public async Task AddUsers()
            => await _model.AddUsersAsync(Users
                .Where(each => each.IsEnabled && each.IsSelected)
                .Select(each => each.Content.Email)
                .ToList());

        public async Task UpdateUsers()
        {
            _model.GetAllUserAsync().Wait();
            await _model.GetAllRoomParticipantsAsync();
        }

        public void RoomParticipantsChanged(UserInformation[] userInformations)
        {
            foreach (var user in _model.AllUser)
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