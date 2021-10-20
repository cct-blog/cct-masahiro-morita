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
        public Guid RoomId { get => _roomId; set => ValueChangeProcess(ref _roomId, value); }

        public UserListViewModel(Chat.IPresenter presenter, Guid roomId, ChatModel model)
        {
            _presenter = presenter;
            _roomId = roomId;
            _model = model;

            _model.RoomParticipantsChanged += (s, e) => RoomParticipantsChanged(e);
        }

        public async Task LeaveRoom()
        {
            var user = await _presenter.GetUserAsync();
            await _model.DeleteUsersAsync(new List<string>() { user.Id });
        }

        public async Task AddUsers()
            => await _model.AddUsersAsync(Users
                .Where(each => each.IsEnabled && each.IsSelected)
                .Select(each => each.Content.Email)
                .ToList());

        public async Task UpdateUsers()
        {
            await _model.GetAllUserAsync();
            await _model.GetAllRoomParticipantsAsync();

            RoomParticipantsChanged(_model.RoomParticipants.ToArray());
        }

        public async Task OnInitializedAsync()
            => await UpdateUsers();

        public void RoomParticipantsChanged(UserInformation[] userInformations)
        {
            var localUsers = userInformations.Select(each => each.Email).ToHashSet();

            Users.Clear();
            foreach (var user in _model.AllUser
                .Select(each =>
                {
                    var exist = localUsers.Contains(each.Email);
                    return new Selectable<UserInformation>
                    {
                        IsSelected = exist,
                        IsEnabled = !exist,
                        Content = each
                    };
                }))
            {
                Users.Add(user);
            }

            _presenter.Invalidate();
        }
    }
}