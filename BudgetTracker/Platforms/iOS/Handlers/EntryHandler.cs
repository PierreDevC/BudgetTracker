using Microsoft.Maui.Handlers;
using UIKit;

namespace BudgetTracker.Platforms.iOS.Handlers
{
    public static class EntryHandlerCustomization
    {
        public static void RemoveNativeBorder()
        {
            EntryHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
            {
                handler.PlatformView.BorderStyle = UITextBorderStyle.None;
            });

            PickerHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
            {
                handler.PlatformView.BorderStyle = UITextBorderStyle.None;
            });

            DatePickerHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
            {
                handler.PlatformView.BorderStyle = UITextBorderStyle.None;
            });
        }
    }
}
