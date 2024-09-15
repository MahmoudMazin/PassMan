
using System.ComponentModel.DataAnnotations;


namespace PassManNew.Models.Shared
{


    public enum State : byte
    {
        [Display(Name = "In Active")]
        InActive = 0,
        [Display(Name = "Active")]
        Active = 1        
    }
    public enum LogType : byte
    {
        [Display(Name = "Login")]
        Login = 0,

        [Display(Name = "Access Web")]
        AccessWeb = 1

    }

    public enum FileType
    {
        [Display(Name = "Avator")]
        Avatar = 1,
        [Display(Name = "Photo")]
        Photo = 2,
        [Display(Name = "Document")]
        Document = 3
    }

  

    public enum City : byte
    {
        Mecca,
        Jeddah,
        Medina,
        Yanbu,
        King_Abdullah_Economic_City,
        Al_Abwa,
        Al_Artaweeiyah,
        Badr,
        Baljurashi,
        Bisha,
        Bareg,
        Buraydah,
        Al_Bahah,
        Buq_a,
        Dammam,
        Dhahran,
        Dhurma,
        Dahaban,
        Diriyah,
        Duba,
        Dumat_Al_Jandal,
        Dawadmi,
        Farasan_city,
        Gatgat,
        Gerrha,
        Gurayat,
        Al_Gweiiyyah,
        Hautat_Sudair,
        Habala,
        Hajrah,
        Haql,
        Al_Hareeq,
        Harmah,
        Hail,
        Hotat_Bani_Tamim,
        Hofuf,
        Huraymila,
        Hafr_Al_Batin,
        Jabal_Umm_al_Ruus,
        Jalajil,
        Al_Jawf,        
        Jizan,
        Jizan_Economic_City,
        Jubail,
        Al_Jafer,
        Khafji,
        Khaybar,       
        Khamis_Mushayt,
        Al_Kharj,
        Knowledge_Economic_City_Medina,
        Khobar,
        Al_Khutt,
        Layla,
        Lihyan,
        Al_Lith,
        Al_Majmaah,
        Mastoorah,
        Al_Mikhwah,
        Al_Mubarraz,
        Al_Mawain,   
        Muzahmiyya,
        Najran,
        Al_Namas,
        Omloj,
        Al_Omran,
        Al_Oyoon,
        Qadeimah,
        Qatif,
        Qaisumah,
        Al_Qunfudhah,
        Rabigh,
        Rafha,
        Ar_Rass,
        Ras_Tanura,
        Riyadh,
        Riyadh_Al_Khabra,
        Rumailah,
        Sabt_Al_Alaya,
        Saihat,
        Safwa_city,
        Sakakah,
        Sharurah,
        Shaqraa,
        Shaybah,
        As_Sulayyil,
        Taif,
        Tabuk,
        Tanomah,
        Tarout,
        Tayma,
        Thadiq,
        Thuwal,
        Thuqbah,
        Turaif,
        Tabarjal,
        Udhailiyah,
        Al_Ula,
        Um_Al_Sahek,
        Unaizah,
        Uqair,
        Uyayna,
        Uyun_AlJiwa,
        Wadi_Al_Dawasir,
        Al_Wajh,   
        Az_Zaimah,
        Zulfi
    }
}
