using ChatApp.Server.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ChatApp.Server.Helper
{
    public static class ExceptionHelper
    {
        /// <summary>
        /// 戻り値有りの処理から、定型的な戻り値を得ます。
        /// </summary>
        public static async Task<(TResult result, ActionResult error)> ExecuteAsync<TResult>(this ControllerBase controller, Func<Task<TResult>> func, Func<bool> isNotFound = null)
        {
            try
            {
                if (isNotFound?.Invoke() ?? false)
                    return (default, controller.NotFound());

                return (await func(), null);

            }
            catch (BusinessException e)
            {
                return (default, controller.BadRequest(e.Message));
            }
            // TODO : ビジネス例外以外で捕まえるべき例外をここに列挙します。(DB接続系のエラーなど、バグではないがシステムエラーに相当するもの)
            catch (Exception e)
            {
                // TODO : Exception をすべてここで握りつぶすべきではありません。
                return (default, controller.InternalServiceError(e));
            }
        }

        /// <summary>
        /// 戻り値無しの処理から、定型的な戻り値を得る。
        /// </summary>
        public static async Task<ActionResult> ExecuteAsync(this ControllerBase controller, Func<Task> func, Func<bool> isNotFound = null)
        {
            try
            {
                if (isNotFound?.Invoke() ?? false)
                    return controller.NotFound();

                await func();
                return controller.NoContent();

            }
            catch (BusinessException e)
            {
                return controller.BadRequest(e.Message);
            }
            // TODO : ビジネス例外以外で捕まえるべき例外をここに列挙します。(DB接続系のエラーなど、バグではないがシステムエラーに相当するもの)
            catch (Exception e)
            {
                return controller.InternalServiceError(e);
            }
        }

        public static async Task<ActionResult<TResult>> AsResultAsync<TResult>(this Task<(TResult result, ActionResult error)> args)
            => (await args).error as ActionResult<TResult> ?? (await args).result;

        private static ActionResult InternalServiceError(this ControllerBase controller, Exception e)
        {
#if DEBUG
            return controller.StatusCode(500, e);
#else
            // TODO : リリース時に出すべき情報は慎重に選ぶ。
            return controller.StatusCode(500, e.Message);
#endif
        }
    }
}
