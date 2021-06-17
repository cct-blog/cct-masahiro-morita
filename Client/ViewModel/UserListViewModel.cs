using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ChatApp.Client.Pages;
using ChatApp.Client.Services;
using ChatApp.Shared.Models;
using Oniqys.Blazor.ViewModel;

namespace ChatApp.Client.ViewModel
{
    public class UserListViewModel : ContentBase
    {
        private readonly Chat.IPresenter _presenter;
        /// <summary>
        /// 全ユーザーと（入室中情報付き）
        /// </summary>
        public ContentCollection<Selectable<UserInformation>> Users { get; } = new();

        private Guid _roomId;

        /// <summary>
        /// チャットルームID
        /// </summary>
        public Guid RoomId { get => _roomId; set => ValueChangeProcess(ref _roomId, value); }

        public UserListViewModel(Chat.IPresenter presenter, Guid roomId)
        {
            _presenter = presenter;
            _roomId = roomId;
        }

        public async Task LeaveRoom()
        {
            var user = await _presenter.GetUserAsync();

            await RoomUtility.DeleteUserFromRoom(RoomId, user.Id, _presenter.GetHttpClient());

            _presenter.GetNavigationManager().NavigateTo("/");
        }

        public async Task AddUsers()
        {
            await RoomUtility.AddUsersToRoom(RoomId, Users.Where(each => each.IsEnabled && each.IsSelected).Select(each => each.Content.Email).ToList(), _presenter.GetHttpClient());

            await UpdateUsers();
        }

        public async Task UpdateUsers()
        {
            var roomDetail = await _presenter.GetHttpClient().GetFromJsonAsync<RoomDetail>($"room/{_roomId}");

            var localUsers = roomDetail.Users.Select(each => each.Email).ToHashSet();

            var allUsers = await _presenter.GetHttpClient().GetFromJsonAsync<List<UserInformation>>($"user");

            Users.Clear();
            foreach (var user in allUsers.Select(each =>
            {
                var exist = localUsers.Contains(each.Email);
                return new Selectable<UserInformation> { IsSelected = exist, IsEnabled = !exist, Content = each };
            }))
            {
                Users.Add(user);
            }
        }

        public async Task OnInitializedAsync()
        {
            await UpdateUsers();
        }
    }
}
