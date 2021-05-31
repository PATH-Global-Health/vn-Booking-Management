using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Constants
{
    public static class RabbitQueue
    {
        public const string InstanceSyncQueue = "InstanceSyncQueue";
        public const string ExaminationBookingQueue = "ExaminationBookingQueue";
        public const string CancelExaminationBookingQueue = "CancelExaminationBookingQueue";
        public const string UpdateBookingStatus = "UpdateBookingStatus";
        public const string NewExaminationBookingQueue = "NewExaminationBookingQueue";
        public const string NewCancelExaminationBookingQueue = "NewCancelExaminationBookingQueue";
        public const string NewUpdateBookingStatus = "NewUpdateBookingStatus";
        public const string BookingIntervalSyncQueue = "BookingIntervalSyncQueue";
    }
}
