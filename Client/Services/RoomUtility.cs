using blazorTest.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace blazorTest.Client.Services
{
    public static class RoomUtility
    {
        /// <summary>
        /// ルームから指定したユーザーを削除します。
        /// </summary>
        /// <param name="roomId">ユーザーを削除するルームのID</param>
        /// <param name="userEmail">`;`で削除するユーザーのEmailを区切った文字列</param>
        /// <param name="httpClient">razorで注入したHttpClientインスタンス</param>
        /// <returns></returns>
        public static async Task DeleteUserFromRoom(Guid roomId, string userEmail, HttpClient httpClient)
        {
            if (!string.IsNullOrEmpty(userEmail))
            {
                if (!userEmail.Contains(";"))
                {
                    string[] userEmailArray = { userEmail };
                    await RoomUtility.DeleteUserFromRoom(roomId, new List<string>(userEmailArray), httpClient);
                }
                else
                {
                    await RoomUtility.DeleteUserFromRoom(roomId, userEmail.Split(";").ToList(), httpClient);
                }
            }
        }

        /// <summary>
        /// ルームから指定したユーザーを削除します。
        /// </summary>
        /// <param name="roomId">ユーザーを削除するルームのID</param>
        /// <param name="userEmails">ルームから削除するユーザーのEmailのリスト</param>
        /// <param name="httpClient">razorで注入したHttpClientインスタンス</param>
        /// <returns></returns>
        public static async Task DeleteUserFromRoom(Guid roomId, List<string> userEmails, HttpClient httpClient)
        {
            var urlBuilder = new System.Text.StringBuilder();
            urlBuilder.Append("Room/");
            urlBuilder.Append(roomId);
            urlBuilder.Append("/User/");
            var urlParts = urlBuilder.ToString();

            userEmails.ForEach(async userEmail =>
                {
                    var url = new System.Text.StringBuilder(urlParts);

                    url.Append(userEmail);

                    await httpClient.DeleteAsync(url.ToString());
                });

            urlBuilder.Clear();
            urlBuilder.Append("Room/");
            urlBuilder.Append(roomId);

            var response = await httpClient.GetAsync(urlBuilder.ToString());
            var responseContent = await response.Content.ReadFromJsonAsync<RoomDetail>();

            if (responseContent.Users?.Count == 0)
            {
                await httpClient.DeleteAsync(urlBuilder.ToString());
            }
        }

        /// <summary>
        /// 指定したユーザーをルームに追加します。
        /// </summary>
        /// <param name="roomId">ルームのID</param>
        /// <param name="userEmails">ルームに追加するユーザーのEmailのリスト</param>
        /// <param name="httpClient">razorで注入したHttpClientインスタンス</param>
        /// <returns></returns>
        public static async Task AddUsersToRoom(Guid roomId, List<string> userEmails, HttpClient httpClient)
        {
            var urlBuilder = new System.Text.StringBuilder();
            urlBuilder.Append("Room/");
            urlBuilder.Append(roomId);
            urlBuilder.Append("/User/");
            var urlParts = urlBuilder.ToString();

            var url = new System.Text.StringBuilder(urlParts);
            await httpClient.PostAsync(url.ToString(), JsonContent.Create(userEmails));
        }

        /// <summary>
        /// ルーム内の投稿を更新します。
        /// ただし、ルームの最終アクセス日時は更新しません。
        /// </summary>
        /// <param name="roomId">投稿を取得したいルームのID</param>
        /// <param name="httpClient">razorで注入したHttpClientインスタンス</param>
        /// <returns>ルーム内の投稿一覧</returns>
        public static async Task<List<Message>> RefreshPostInRoom(Guid roomId, HttpClient httpClient)
        {
            var request = new ChatPostPostRequest()
            {
                RoomId = roomId,
                NeedMessageTailDate = DateTime.Now
            };

            var response = await httpClient.PostAsJsonAsync<ChatPostPostRequest>("Post", request);

            return await response.Content.ReadFromJsonAsync<List<Message>>();
        }

        /// <summary>
        /// ルーム内の投稿を更新します。
        /// ルームの最終アクセス日時も更新します。
        /// </summary>
        /// <param name="roomId">投稿を取得したいルームのID</param>
        /// <param name="httpClient">razorで注入したHttpClientインスタンス</param>
        /// <returns>ルーム内の投稿一覧</returns>
        public static async Task<List<Message>> InitializeRoom(Guid roomId, HttpClient httpClient)
        {
            var messages = await RefreshPostInRoom(roomId, httpClient);

            var url = new System.Text.StringBuilder();
            url.Append("Room/");
            url.Append(roomId);

            await httpClient.PutAsync(url.ToString(), null);

            return messages;
        }
    }
}