using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSollaris
{
    static public class OtherHelper
    {
        /// <summary>
        /// Allows to recursively lock on multiple objects to block multiple threads independently of their count
        /// </summary>
        /// <param name="lockers">Array of lockers to be locked</param>
        /// <param name="counter">Current object to lock, used for recursion</param>
        /// <param name="action">Action to perform after all locks have been obtained</param>
        public static void MultiLock(object[] lockers, int counter, Action action)
        {
            if (counter > 0)
            {
                counter--;
                lock (lockers[counter])
                {
                    MultiLock(lockers, counter, action);
                }
            }
            else action.Invoke();
        }
    }
}
