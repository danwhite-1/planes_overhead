using ResponseJsonLib;

namespace Notification
{
    public class NotificationResponse : Response
    {
        //! Error contructor
        public NotificationResponse(int _code, string _errMsg) : base(_code, _errMsg) { }

        //! Success constructor
        public NotificationResponse(int _emailsSent) : base(200)
        {
            emailsSent = _emailsSent;
        }

        public int emailsSent;
    }
}