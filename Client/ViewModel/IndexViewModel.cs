using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Client.Models;
using Oniqys.Blazor.ViewModel;
using static ChatApp.Client.Shared.MainLayout;

namespace ChatApp.Client.ViewModel
{
    public class IndexViewModel : ContentBase
    {
        private readonly IndexModel _model;

        public ContentCollection<RoomModel> Rooms = new();

        private readonly IIndexPresenter _presenter;

        public IndexViewModel(IIndexPresenter presenter)
        {
            _presenter = presenter;
            _model = new IndexModel(_presenter.GetHttpClientFactory());

            _model.RoomListChanged += (s, e) => OnRoomChanged(e);
        }

        private void OnRoomChanged(RoomModel[] rooms)
        {
            var oldRoomIds = Rooms.Select(each => each.RoomId).ToHashSet();
            var newRoomIds = rooms.Select(each => each.RoomId).ToHashSet();

            var updateRoomIds = Rooms
                .Select(each => each.RoomId)
                .Intersect(rooms.Select(each => each.RoomId).ToHashSet());

            var deleteRoomIds = oldRoomIds.Except(updateRoomIds);
            var addRoomIds = newRoomIds.Except(updateRoomIds);

            foreach (var updateRoomId in updateRoomIds)
            {
                var target = Rooms.FirstOrDefault(each => each.RoomId == updateRoomId);
                target = rooms.FirstOrDefault(each => each.RoomId == updateRoomId);
            }

            foreach (var deleteRoomId in deleteRoomIds)
            {
                var target = Rooms.FirstOrDefault(each => each.RoomId == deleteRoomId);
                Rooms.Remove(target);
            }

            foreach (var addRoomId in addRoomIds)
            {
                var target = rooms.FirstOrDefault(each => each.RoomId == addRoomId);
                Rooms.Add(target);
            }

            _presenter.Invalidate();
        }

        public async Task CreateRoomAsync(string roomName, List<string> userEmails)
            => await _model.CreateRoomAsync(roomName, userEmails);

        public async Task UpdateRoomList()
            => await _model.GetUserBelongedRoomsAsync();
    }
}