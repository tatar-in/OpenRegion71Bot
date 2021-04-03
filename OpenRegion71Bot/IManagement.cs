using System;
using System.Collections.Generic;
using System.Text;

namespace OpenRegion71Bot
{
    interface IManagement
    {
        /// <summary>
        /// Remove the message from delinquency
        /// </summary>
        /// <param name="id">ID of the message</param>
        void RemoveFromDelinquency(int id);

        /// <summary>
        /// Change the executor
        /// </summary>
        /// <param name="id">ID of the message</param>
        /// <param name="executor">ID of the executor</param>
        void ChangeExecutor(int id, int executor);

        /// <summary>
        /// Extend the deadline
        /// </summary>
        /// <param name="id">ID of the message</param>
        /// <param name="date">New deadline date</param>
        void ExtendDeadline(int id, DateTime date);

        /// <summary>
        /// Change the theme
        /// </summary>
        /// <param name="id">ID of the message</param>
        /// <param name="theme">ID of the theme</param>
        void ChangeTheme(int id, int theme);    
    }
}
