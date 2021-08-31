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

    public enum TestingType : int
    {
        LAY_TEST  =1,
        VIRAL_LOAD = 2,
        CD4 = 3,
        RECENCY = 4,
        HTS_POS = 5
    }

    public enum ResultTesting : int
    {
        POSITIVE = 1,
        NEGATIVE = -1,
        NO_RESULT = 0
    } 



}
