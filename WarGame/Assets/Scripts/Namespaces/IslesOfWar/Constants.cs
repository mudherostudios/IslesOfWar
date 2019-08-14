using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IslesOfWar
{
    public static class Constants
    {
        public static int[] version = new int[] {0, 0, 0}; //Compatibility GSP Version, Effeciency Version, Client Version

        public static Dictionary<string, string> countryCodes = new Dictionary<string, string>()
        {
            {"AF","Afghanistan"},{"AX","Ãland Islands"},{"AL","Albania"},{"DZ","Algeria"},{"AS","American Samoa"},
            {"AD","Andorra"},{"AO","Angola"},{"AI","Anguilla"},{"AQ","Antarctica"},{"AG","Antigua and Barbuda"},
            {"AR","Argentina"},{"AM","Armenia"},{"AW","Aruba"},{"AU","Australia"},{"AT","Austria"},{"AZ","Azerbaijan"},
            {"BS","Bahamas"},{"BH","Bahrain"},{"BD","Bangladesh"},{"BB","Barbados"},{"BY","Belarus"},{"BE","Belgium"},
            {"BZ","Belize"},{"BJ","Benin"},{"BM","Bermuda"},{"BT","Bhutan"},{"BO","Bolivia,Plurinational State of"},
            {"BQ","Bonaire,Sint Eustatius and Saba"},{"BA","Bosnia and Herzegovina"},{"BW","Botswana"},{"BV","Bouvet Island"},
            {"BR","Brazil"},{"IO","British Indian Ocean Territory"},{"BN","Brunei Darussalam"},{"BG","Bulgaria"},{"BF","Burkina Faso"},
            {"BI","Burundi"},{"KH","Cambodia"},{"CM","Cameroon"},{"CA","Canada"},{"CV","Cape Verde"},{"KY","Cayman Islands"},
            {"CF","Central African Republic"},{"TD","Chad"},{"CL","Chile"},{"CN","China"},{"CX","Christmas Island"},
            {"CC","Cocos (Keeling) Islands"},{"CO","Colombia"},{"KM","Comoros"},{"CG","Congo"},
            {"CD","Congo,the Democratic Republic of the"},{"CK","Cook Islands"},{"CR","Costa Rica"},{"CI","CÃ´te d'Ivoire"},
            {"HR","Croatia"},{"CU","Cuba"},{"CW","CuraÃ§ao"},{"CY","Cyprus"},{"CZ","Czech Republic"},{"DK","Denmark"},
            {"DJ","Djibouti"},{"DM","Dominica"},{"DO","Dominican Republic"},{"EC","Ecuador"},{"EG","Egypt"},
            {"SV","El Salvador"},{"GQ","Equatorial Guinea"},{"ER","Eritrea"},{"EE","Estonia"},{"ET","Ethiopia"},
            {"FK","Falkland Islands (Malvinas)"},{"FO","Faroe Islands"},{"FJ","Fiji"},{"FI","Finland"},{"FR","France"},
            {"GF","French Guiana"},{"PF","French Polynesia"},{"TF","French Southern Territories"},{"GA","Gabon"},{"GM","Gambia"},
            {"GE","Georgia"},{"DE","Germany"},{"GH","Ghana"},{"GI","Gibraltar"},{"GR","Greece"},{"GL","Greenland"},
            {"GD","Grenada"},{"GP","Guadeloupe"},{"GU","Guam"},{"GT","Guatemala"},{"GG","Guernsey"},{"GN","Guinea"},
            {"GW","Guinea-Bissau"},{"GY","Guyana"},{"HT","Haiti"},{"HM","Heard Island and McDonald Islands"},
            {"VA","Holy See (Vatican City State)"},{"HN","Honduras"},{"HK","Hong Kong"},{"HU","Hungary"},{"IS","Iceland"},
            {"IN","India"},{"ID","Indonesia"},{"IR","Iran,Islamic Republic of"},{"IQ","Iraq"},{"IE","Ireland"},{"IM","Isle of Man"},
            {"IL","Israel"},{"IT","Italy"},{"JM","Jamaica"},{"JP","Japan"},{"JE","Jersey"},{"JO","Jordan"},{"KZ","Kazakhstan"},
            {"KE","Kenya"},{"KI","Kiribati"},{"KP","Korea,Democratic People's Republic of"},{"KR","Korea,Republic of"},{"KW","Kuwait"},
            {"KG","Kyrgyzstan"},{"LA","Lao People's Democratic Republic"},{"LV","Latvia"},{"LB","Lebanon"},{"LS","Lesotho"},
            {"LR","Liberia"},{"LY","Libya"},{"LI","Liechtenstein"},{"LT","Lithuania"},{"LU","Luxembourg"},{"MO","Macao"},
            {"MK","Macedonia,the Former Yugoslav Republic of"},{"MG","Madagascar"},{"MW","Malawi"},{"MY","Malaysia"},{"MV","Maldives"},
            {"ML","Mali"},{"MT","Malta"},{"MH","Marshall Islands"},{"MQ","Martinique"},{"MR","Mauritania"},{"MU","Mauritius"},
            {"YT","Mayotte"},{"MX","Mexico"},{"FM","Micronesia,Federated States of"},{"MD","Moldova,Republic of"},{"MC","Monaco"},
            {"MN","Mongolia"},{"ME","Montenegro"},{"MS","Montserrat"},{"MA","Morocco"},{"MZ","Mozambique"},{"MM","Myanmar"},
            {"NA","Namibia"},{"NR","Nauru"},{"NP","Nepal"},{"NL","Netherlands"},{"NC","New Caledonia"},{"NZ","New Zealand"},
            {"NI","Nicaragua"},{"NE","Niger"},{"NG","Nigeria"},{"NU","Niue"},{"NF","Norfolk Island"},{"MP","Northern Mariana Islands"},
            {"NO","Norway"},{"OM","Oman"},{"PK","Pakistan"},{"PW","Palau"},{"PS","Palestine,State of"},{"PA","Panama"},
            {"PG","Papua New Guinea"},{"PY","Paraguay"},{"PE","Peru"},{"PH","Philippines"},{"PN","Pitcairn"},{"PL","Poland"},
            {"PT","Portugal"},{"PR","Puerto Rico"},{"QA","Qatar"},{"RE","RÃ©union"},{"RO","Romania"},{"RU","Russian Federation"},
            {"RW","Rwanda"},{"BL","Saint BarthÃ©lemy"},{"SH","Saint Helena,Ascension and Tristan da Cunha"},{"KN","Saint Kitts and Nevis"},
            {"LC","Saint Lucia"},{"MF","Saint Martin (French part)"},{"PM","Saint Pierre and Miquelon"},{"VC","Saint Vincent and the Grenadines"},
            {"WS","Samoa"},{"SM","San Marino"},{"ST","Sao Tome and Principe"},{"SA","Saudi Arabia"},{"SN","Senegal"},{"RS","Serbia"},
            {"SC","Seychelles"},{"SL","Sierra Leone"},{"SG","Singapore"},{"SX","Sint Maarten (Dutch part)"},{"SK","Slovakia"},
            {"SI","Slovenia"},{"SB","Solomon Islands"},{"SO","Somalia"},{"ZA","South Africa"},{"GS","South Georgia and the South Sandwich Islands"},
            {"SS","South Sudan"},{"ES","Spain"},{"LK","Sri Lanka"},{"SD","Sudan"},{"SR","Suriname"},{"SJ","Svalbard and Jan Mayen"},
            {"SZ","Swaziland"},{"SE","Sweden"},{"CH","Switzerland"},{"SY","Syrian Arab Republic"},{"TW","Taiwan,Province of China"},
            {"TJ","Tajikistan"},{"TZ","Tanzania,United Republic of"},{"TH","Thailand"},{"TL","Timor-Leste"},{"TG","Togo"},{"TK","Tokelau"},
            {"TO","Tonga"},{"TT","Trinidad and Tobago"},{"TN","Tunisia"},{"TR","Turkey"},{"TM","Turkmenistan"},{"TC","Turks and Caicos Islands"},
            {"TV","Tuvalu"},{"UG","Uganda"},{"UA","Ukraine"},{"AE","United Arab Emirates"},{"GB","United Kingdom"},{"US","United States"},
            {"UM","United States Minor Outlying Islands"},{"UY","Uruguay"},{"UZ","Uzbekistan"},{"VU","Vanuatu"},
            {"VE","Venezuela,Bolivarian Republic of"},{"VN","Viet Nam"},{"VG","Virgin Islands,British"},{"VI","Virgin Islands,U.S."},
            {"WF","Wallis and Futuna"},{"EH","Western Sahara"},{"YE","Yemen"},{"ZM","Zambia"},{"ZW","Zimbabwe"}
        };

        public static int[] islandSearchCost = new int[] { 1000, 2500, 0, 0 };
        
        //Warbucks, Oil, Metal, No concrete because no unit cost concrete (maybe).
        public static int[,] unitCosts = new int[,]
        {
            {10,    0,      10,     0},
            {50,    0,      20,     0},
            {100,   10,     20,     0},
            {100,   25,     100,    0},
            {200,   50,     200,    0},
            {500,   100,    500,    0},
            {250,   100,    50,     0},
            {500,   250,    75,     0},
            {1000,  500,    200,    0}
        };

        public static int[,] blockerCosts = new int[,]
        {
            {1500, 100, 1000, 100 },
            {1500, 1000, 500, 500 },
            {1500, 500, 1000, 1000}
        };

        public static int[,] bunkerCosts = new int[,]
        {
            {1500, 100, 1000, 100 },
            {1500, 1000, 500, 500 },
            {1500, 500, 1000, 1000}
        };

        public static int[,] collectorCosts = new int[,]
        {
            {1500, 500, 1000, 1000 },
            {1500, 1000, 500, 1000 },
            {1500, 1000, 1000, 500 }
        };

        public static float[] unitDamages = new float[]
        {
            2.0f, 3.0f, 4.0f,
            2.5f, 5.0f, 10.0f,
            8.0f, 12.0f, 14.0f,
            12.0f, 30.0f, 16.0f
        };

        public static float[] unitHealths = new float[]
        {
            100.0f, 100.0f, 100.0f,
            125.0f, 250.0f, 500.0f,
            200.0f, 300.0f, 200.0f,
            300.0f, 750.0f, 400.0f
        };

        public static float[] unitOrderProbabilities = new float[]
        {
            0.5f, 0.4f, 0.3f,
            0.1f, 0.085f, 0.065f,
            0.05f, 0.05f, 0.05f,
            0.01f, 0.01f, 0.01f
        };
        
        //12x12 grid - troop, machine, zook, lTank, mTank, hTank, lPlane, mPlane, bomber, troopBunk, tankBunk, airBunk
        public static float[,] unitCombatModifiers = new float[,]
        {
            {1.0f, 1.0f, 1.0f,     0.1f, 0.1f, 0.1f,    0.1f, 0.1f, 0.1f,     0.1f, 0.1f, 0.1f},
            {1.5f, 1.5f, 1.5f,     0.25f, 0.1f, 0.1f,   0.25f, 0.25f, 0.25f,  0.1f, 0.1f, 0.1f},
            {0.1f, 0.1f, 0.1f,     1.5f, 1.25f, 1.0f,   1.5f, 1.5f, 1.5f,     1.0f, 1.0f, 1.0f},
            {1.0f, 1.0f, 1.0f,     1.0f, 0.5f, 0.25f,   0.3f, 0.15f, 0.1f,    1.5f, 0.5f, 1.5f},
            {0.75f, 0.75f, 0.75f,  0.6f, 0.3f, 0.1f,    1.5f, 1.0f, 0.5f,     1.5f, 0.5f, 1.5f},
            {0.5f, 0.5f, 0.5f,     2.0f, 1.5f, 1.0f,    0.75f, 0.5f, 0.2f,    1.5f, 0.5f, 1.5f},
            {2.0f, 2.0f, 2.0f,     2.0f, 1.5f, 1.0f,    1.0f, 1.5f, 0.75f,    0.5f, 0.5f, 0.5f},
            {1.5f, 1.5f, 1.5f,     2.0f, 1.5f, 1.0f,    0.5f, 1.0f, 1.5f,     1.0f, 1.0f, 1.0f},
            {1.0f, 1.0f, 1.0f,     1.5f, 1.5f, 1.5f,    0.1f, 0.1f, 0.1f,     2.0f, 2.0f, 2.0f},
            {2.0f, 2.0f, 2.0f,     1.5f, 0.25f, 0.25f,  2.25f, 1.5f, 1.5f,    0.0f, 0.0f, 0.0f},
            {1.5f, 1.5f, 1.5f,     4.0f, 3.0f, 2.0f,    0.3f, 0.2f, 0.1f,     0.0f, 0.0f, 0.0f},
            {1.0f, 1.0f, 1.0f,     1.5f, 0.25f, 0.25f,  4.0f, 3.5f, 2.0f,     0.0f, 0.0f, 0.0f}
        };

        public static float[,] minMaxResources = new float[,]
        {
            {1000, 100000},
            {1000, 100000},
            {1000, 100000}
        };

        public static float[] extractRates = new float[] { 10, 20, 30 };
        public static float[] freeResourceRates = new float[] {1, 2, 3, 4 };
    }
}
