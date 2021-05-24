using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Enums
{
    public enum MedicalTestStatus : int
    {
        UNFINISHED = 1,
        FINISHED = 2,
        CANCELED = 3,
        NOT_DOING = 4,
        DOCTOR_CANCEL = 5,
    }

    public enum BookingStatus : int
    {
        UNFINISHED = 1,
        FINISHED = 2,
        CANCELED = 3,
        NOT_DOING = 4,
        DOCTOR_CANCEL = 5,
        RESULTED = 6
    }
}
