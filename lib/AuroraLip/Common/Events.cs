namespace AuroraLib.Common
{
    /// <summary>
    /// Events that make it possible to influence the behavior of the library.
    /// </summary>
    public static class Events
    {
        /// <summary>
        /// Event that is called when a process wants to report something.
        /// </summary>
        public static NotificationDelegate NotificationEvent = DefaultNotification;

        /// <summary>
        /// Represents the method that will handle the NotificationEvent.
        /// </summary>
        /// <param name="type">Type of notification.</param>
        /// <param name="message">The notification message.</param>
        public delegate void NotificationDelegate(NotificationType type, string message);

        /// <summary>
        /// Represents the method to request a missing file.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public delegate Stream FileRequestDelegate(string Name);

        private static void DefaultNotification(NotificationType type, string message)
            => Console.WriteLine($"{type}: {message}");

    }
}
