using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TurboRent.Modules.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandAzerbaijanVehicleCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "catalog",
                table: "vehicle_makes",
                columns: new[] { "Id", "NameAz", "NameEn", "Slug" },
                values: new object[,]
                {
                    { 7, "Audi", "Audi", "audi" },
                    { 8, "Volkswagen", "Volkswagen", "volkswagen" },
                    { 9, "Ford", "Ford", "ford" },
                    { 10, "Chevrolet", "Chevrolet", "chevrolet" },
                    { 11, "Honda", "Honda", "honda" },
                    { 12, "Lexus", "Lexus", "lexus" },
                    { 13, "Porsche", "Porsche", "porsche" },
                    { 14, "Land Rover", "Land Rover", "land-rover" },
                    { 15, "Mazda", "Mazda", "mazda" },
                    { 16, "Mitsubishi", "Mitsubishi", "mitsubishi" },
                    { 17, "Subaru", "Subaru", "subaru" },
                    { 18, "Suzuki", "Suzuki", "suzuki" },
                    { 19, "Infiniti", "Infiniti", "infiniti" },
                    { 20, "Renault", "Renault", "renault" },
                    { 21, "Peugeot", "Peugeot", "peugeot" },
                    { 22, "Citroen", "Citroen", "citroen" },
                    { 23, "Opel", "Opel", "opel" },
                    { 24, "Skoda", "Skoda", "skoda" },
                    { 25, "Volvo", "Volvo", "volvo" },
                    { 26, "Jaguar", "Jaguar", "jaguar" },
                    { 27, "Jeep", "Jeep", "jeep" },
                    { 28, "Tesla", "Tesla", "tesla" },
                    { 29, "BYD", "BYD", "byd" },
                    { 30, "Chery", "Chery", "chery" },
                    { 31, "Geely", "Geely", "geely" },
                    { 32, "Haval", "Haval", "haval" },
                    { 33, "Changan", "Changan", "changan" },
                    { 34, "GAC", "GAC", "gac" },
                    { 35, "JAC", "JAC", "jac" },
                    { 36, "Jetour", "Jetour", "jetour" },
                    { 37, "Exeed", "Exeed", "exeed" },
                    { 38, "Hongqi", "Hongqi", "hongqi" },
                    { 39, "Dongfeng", "Dongfeng", "dongfeng" },
                    { 40, "FAW", "FAW", "faw" },
                    { 41, "Li Auto", "Li Auto", "li-auto" },
                    { 42, "Zeekr", "Zeekr", "zeekr" },
                    { 43, "XPeng", "XPeng", "xpeng" },
                    { 44, "Omoda", "Omoda", "omoda" },
                    { 45, "Jaecoo", "Jaecoo", "jaecoo" },
                    { 46, "Tank", "Tank", "tank" },
                    { 47, "MG", "MG", "mg" },
                    { 48, "Great Wall", "Great Wall", "great-wall" },
                    { 49, "Lada", "Lada", "lada" },
                    { 50, "BAIC", "BAIC", "baic" },
                    { 51, "Leapmotor", "Leapmotor", "leapmotor" },
                    { 52, "NIO", "NIO", "nio" },
                    { 53, "Voyah", "Voyah", "voyah" },
                    { 54, "Seres", "Seres", "seres" },
                    { 55, "Dacia", "Dacia", "dacia" },
                    { 56, "Fiat", "Fiat", "fiat" },
                    { 57, "Alfa Romeo", "Alfa Romeo", "alfa-romeo" },
                    { 58, "Bentley", "Bentley", "bentley" },
                    { 59, "Rolls-Royce", "Rolls-Royce", "rolls-royce" },
                    { 60, "Maserati", "Maserati", "maserati" },
                    { 61, "Acura", "Acura", "acura" },
                    { 62, "Lincoln", "Lincoln", "lincoln" },
                    { 63, "Dodge", "Dodge", "dodge" },
                    { 64, "SEAT", "SEAT", "seat" },
                    { 65, "Smart", "Smart", "smart" },
                    { 66, "KGM", "KGM", "kgm" },
                    { 67, "Lynk & Co", "Lynk & Co", "lynk-and-co" },
                    { 68, "Daewoo", "Daewoo", "daewoo" },
                    { 69, "Daihatsu", "Daihatsu", "daihatsu" },
                    { 70, "Buick", "Buick", "buick" },
                    { 71, "GMC", "GMC", "gmc" },
                    { 72, "Isuzu", "Isuzu", "isuzu" },
                    { 73, "Chrysler", "Chrysler", "chrysler" },
                    { 74, "Cupra", "Cupra", "cupra" },
                    { 75, "Ram", "Ram", "ram" }
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "vehicle_models",
                columns: new[] { "Id", "MakeId", "NameAz", "NameEn", "Slug" },
                values: new object[,]
                {
                    { 19, 1, "Land Cruiser", "Land Cruiser", "land-cruiser" },
                    { 20, 1, "Land Cruiser Prado", "Land Cruiser Prado", "land-cruiser-prado" },
                    { 21, 1, "RAV4", "RAV4", "rav4" },
                    { 22, 1, "Highlander", "Highlander", "highlander" },
                    { 23, 1, "Yaris", "Yaris", "yaris" },
                    { 24, 1, "Yaris Cross", "Yaris Cross", "yaris-cross" },
                    { 25, 1, "Hilux", "Hilux", "hilux" },
                    { 26, 1, "Fortuner", "Fortuner", "fortuner" },
                    { 27, 1, "C-HR", "C-HR", "c-hr" },
                    { 28, 1, "Corolla Cross", "Corolla Cross", "corolla-cross" },
                    { 29, 1, "Avalon", "Avalon", "avalon" },
                    { 30, 1, "Venza", "Venza", "venza" },
                    { 31, 1, "Harrier", "Harrier", "harrier" },
                    { 32, 1, "Crown", "Crown", "crown" },
                    { 33, 2, "Santa Fe", "Santa Fe", "santa-fe" },
                    { 34, 2, "Palisade", "Palisade", "palisade" },
                    { 35, 2, "Accent", "Accent", "accent" },
                    { 36, 2, "i10", "i10", "i10" },
                    { 37, 2, "i20", "i20", "i20" },
                    { 38, 2, "i30", "i30", "i30" },
                    { 39, 2, "Kona", "Kona", "kona" },
                    { 40, 2, "Venue", "Venue", "venue" },
                    { 41, 2, "Creta", "Creta", "creta" },
                    { 42, 2, "Staria", "Staria", "staria" },
                    { 43, 2, "Ioniq 5", "Ioniq 5", "ioniq-5" },
                    { 44, 2, "Ioniq 6", "Ioniq 6", "ioniq-6" },
                    { 45, 2, "Bayon", "Bayon", "bayon" },
                    { 46, 2, "Mufasa", "Mufasa", "mufasa" },
                    { 47, 3, "Sorento", "Sorento", "sorento" },
                    { 48, 3, "Seltos", "Seltos", "seltos" },
                    { 49, 3, "Picanto", "Picanto", "picanto" },
                    { 50, 3, "Cerato", "Cerato", "cerato" },
                    { 51, 3, "K5", "K5", "k5" },
                    { 52, 3, "K8", "K8", "k8" },
                    { 53, 3, "K9", "K9", "k9" },
                    { 54, 3, "Carnival", "Carnival", "carnival" },
                    { 55, 3, "Telluride", "Telluride", "telluride" },
                    { 56, 3, "Stinger", "Stinger", "stinger" },
                    { 57, 3, "Niro", "Niro", "niro" },
                    { 58, 3, "EV3", "EV3", "ev3" },
                    { 59, 3, "EV5", "EV5", "ev5" },
                    { 60, 3, "EV6", "EV6", "ev6" },
                    { 61, 3, "EV9", "EV9", "ev9" },
                    { 62, 4, "A-Class", "A-Class", "a-class" },
                    { 63, 4, "S-Class", "S-Class", "s-class" },
                    { 64, 4, "CLA", "CLA", "cla" },
                    { 65, 4, "CLS", "CLS", "cls" },
                    { 66, 4, "GLA", "GLA", "gla" },
                    { 67, 4, "GLB", "GLB", "glb" },
                    { 68, 4, "GLE", "GLE", "gle" },
                    { 69, 4, "GLS", "GLS", "gls" },
                    { 70, 4, "G-Class", "G-Class", "g-class" },
                    { 71, 4, "V-Class", "V-Class", "v-class" },
                    { 72, 4, "EQA", "EQA", "eqa" },
                    { 73, 4, "EQB", "EQB", "eqb" },
                    { 74, 4, "EQE", "EQE", "eqe" },
                    { 75, 4, "EQS", "EQS", "eqs" },
                    { 76, 5, "1 Series", "1 Series", "1-series" },
                    { 77, 5, "2 Series", "2 Series", "2-series" },
                    { 78, 5, "4 Series", "4 Series", "4-series" },
                    { 79, 5, "6 Series", "6 Series", "6-series" },
                    { 80, 5, "7 Series", "7 Series", "7-series" },
                    { 81, 5, "8 Series", "8 Series", "8-series" },
                    { 82, 5, "X1", "X1", "x1" },
                    { 83, 5, "X2", "X2", "x2" },
                    { 84, 5, "X3", "X3", "x3" },
                    { 85, 5, "X4", "X4", "x4" },
                    { 86, 5, "X6", "X6", "x6" },
                    { 87, 5, "X7", "X7", "x7" },
                    { 88, 5, "XM", "XM", "xm" },
                    { 89, 5, "i3", "i3", "i3" },
                    { 90, 5, "i4", "i4", "i4" },
                    { 91, 5, "i5", "i5", "i5" },
                    { 92, 5, "i7", "i7", "i7" },
                    { 93, 5, "iX", "iX", "ix" },
                    { 94, 6, "Maxima", "Maxima", "maxima" },
                    { 95, 6, "Sentra", "Sentra", "sentra" },
                    { 96, 6, "Sunny", "Sunny", "sunny" },
                    { 97, 6, "Micra", "Micra", "micra" },
                    { 98, 6, "Note", "Note", "note" },
                    { 99, 6, "Juke", "Juke", "juke" },
                    { 100, 6, "Pathfinder", "Pathfinder", "pathfinder" },
                    { 101, 6, "Patrol", "Patrol", "patrol" },
                    { 102, 6, "Murano", "Murano", "murano" },
                    { 103, 6, "Kicks", "Kicks", "kicks" },
                    { 104, 6, "Leaf", "Leaf", "leaf" },
                    { 105, 6, "Teana", "Teana", "teana" },
                    { 106, 6, "Terrano", "Terrano", "terrano" },
                    { 107, 7, "A1", "A1", "a1" },
                    { 108, 7, "A3", "A3", "a3" },
                    { 109, 7, "A4", "A4", "a4" },
                    { 110, 7, "A5", "A5", "a5" },
                    { 111, 7, "A6", "A6", "a6" },
                    { 112, 7, "A7", "A7", "a7" },
                    { 113, 7, "A8", "A8", "a8" },
                    { 114, 7, "Q2", "Q2", "q2" },
                    { 115, 7, "Q3", "Q3", "q3" },
                    { 116, 7, "Q5", "Q5", "q5" },
                    { 117, 7, "Q7", "Q7", "q7" },
                    { 118, 7, "Q8", "Q8", "q8" },
                    { 119, 7, "TT", "TT", "tt" },
                    { 120, 7, "e-tron", "e-tron", "e-tron" },
                    { 121, 7, "Q4 e-tron", "Q4 e-tron", "q4-e-tron" },
                    { 122, 7, "Q8 e-tron", "Q8 e-tron", "q8-e-tron" },
                    { 123, 8, "Polo", "Polo", "polo" },
                    { 124, 8, "Golf", "Golf", "golf" },
                    { 125, 8, "Jetta", "Jetta", "jetta" },
                    { 126, 8, "Passat", "Passat", "passat" },
                    { 127, 8, "Arteon", "Arteon", "arteon" },
                    { 128, 8, "Tiguan", "Tiguan", "tiguan" },
                    { 129, 8, "Touareg", "Touareg", "touareg" },
                    { 130, 8, "T-Roc", "T-Roc", "t-roc" },
                    { 131, 8, "T-Cross", "T-Cross", "t-cross" },
                    { 132, 8, "Taos", "Taos", "taos" },
                    { 133, 8, "Teramont", "Teramont", "teramont" },
                    { 134, 8, "ID.3", "ID.3", "id-3" },
                    { 135, 8, "ID.4", "ID.4", "id-4" },
                    { 136, 8, "ID.6", "ID.6", "id-6" },
                    { 137, 8, "ID.7", "ID.7", "id-7" },
                    { 138, 9, "Fiesta", "Fiesta", "fiesta" },
                    { 139, 9, "Focus", "Focus", "focus" },
                    { 140, 9, "Mondeo", "Mondeo", "mondeo" },
                    { 141, 9, "Fusion", "Fusion", "fusion" },
                    { 142, 9, "Taurus", "Taurus", "taurus" },
                    { 143, 9, "Mustang", "Mustang", "mustang" },
                    { 144, 9, "Kuga", "Kuga", "kuga" },
                    { 145, 9, "Escape", "Escape", "escape" },
                    { 146, 9, "Edge", "Edge", "edge" },
                    { 147, 9, "Explorer", "Explorer", "explorer" },
                    { 148, 9, "Bronco", "Bronco", "bronco" },
                    { 149, 9, "EcoSport", "EcoSport", "ecosport" },
                    { 150, 9, "Territory", "Territory", "territory" },
                    { 151, 9, "Everest", "Everest", "everest" },
                    { 152, 10, "Spark", "Spark", "spark" },
                    { 153, 10, "Aveo", "Aveo", "aveo" },
                    { 154, 10, "Cobalt", "Cobalt", "cobalt" },
                    { 155, 10, "Cruze", "Cruze", "cruze" },
                    { 156, 10, "Malibu", "Malibu", "malibu" },
                    { 157, 10, "Impala", "Impala", "impala" },
                    { 158, 10, "Camaro", "Camaro", "camaro" },
                    { 159, 10, "Trax", "Trax", "trax" },
                    { 160, 10, "Tracker", "Tracker", "tracker" },
                    { 161, 10, "Equinox", "Equinox", "equinox" },
                    { 162, 10, "Blazer", "Blazer", "blazer" },
                    { 163, 10, "Traverse", "Traverse", "traverse" },
                    { 164, 10, "Tahoe", "Tahoe", "tahoe" },
                    { 165, 10, "Captiva", "Captiva", "captiva" },
                    { 166, 11, "Civic", "Civic", "civic" },
                    { 167, 11, "Accord", "Accord", "accord" },
                    { 168, 11, "CR-V", "CR-V", "cr-v" },
                    { 169, 11, "HR-V", "HR-V", "hr-v" },
                    { 170, 11, "Pilot", "Pilot", "pilot" },
                    { 171, 11, "City", "City", "city" },
                    { 172, 11, "Fit", "Fit", "fit" },
                    { 173, 11, "Jazz", "Jazz", "jazz" },
                    { 174, 11, "Odyssey", "Odyssey", "odyssey" },
                    { 175, 11, "Passport", "Passport", "passport" },
                    { 176, 11, "ZR-V", "ZR-V", "zr-v" },
                    { 177, 11, "e:Ny1", "e:Ny1", "e-ny1" },
                    { 178, 12, "IS", "IS", "is" },
                    { 179, 12, "ES", "ES", "es" },
                    { 180, 12, "GS", "GS", "gs" },
                    { 181, 12, "LS", "LS", "ls" },
                    { 182, 12, "UX", "UX", "ux" },
                    { 183, 12, "NX", "NX", "nx" },
                    { 184, 12, "RX", "RX", "rx" },
                    { 185, 12, "GX", "GX", "gx" },
                    { 186, 12, "LX", "LX", "lx" },
                    { 187, 12, "LC", "LC", "lc" },
                    { 188, 12, "LM", "LM", "lm" },
                    { 189, 12, "RZ", "RZ", "rz" },
                    { 190, 12, "LBX", "LBX", "lbx" },
                    { 191, 13, "718 Cayman", "718 Cayman", "718-cayman" },
                    { 192, 13, "718 Boxster", "718 Boxster", "718-boxster" },
                    { 193, 13, "911", "911", "911" },
                    { 194, 13, "Panamera", "Panamera", "panamera" },
                    { 195, 13, "Macan", "Macan", "macan" },
                    { 196, 13, "Cayenne", "Cayenne", "cayenne" },
                    { 197, 13, "Taycan", "Taycan", "taycan" },
                    { 198, 14, "Defender", "Defender", "defender" },
                    { 199, 14, "Discovery", "Discovery", "discovery" },
                    { 200, 14, "Discovery Sport", "Discovery Sport", "discovery-sport" },
                    { 201, 14, "Range Rover", "Range Rover", "range-rover" },
                    { 202, 14, "Range Rover Sport", "Range Rover Sport", "range-rover-sport" },
                    { 203, 14, "Range Rover Velar", "Range Rover Velar", "range-rover-velar" },
                    { 204, 14, "Range Rover Evoque", "Range Rover Evoque", "range-rover-evoque" },
                    { 205, 15, "Mazda2", "Mazda2", "mazda2" },
                    { 206, 15, "Mazda3", "Mazda3", "mazda3" },
                    { 207, 15, "Mazda6", "Mazda6", "mazda6" },
                    { 208, 15, "CX-3", "CX-3", "cx-3" },
                    { 209, 15, "CX-30", "CX-30", "cx-30" },
                    { 210, 15, "CX-5", "CX-5", "cx-5" },
                    { 211, 15, "CX-50", "CX-50", "cx-50" },
                    { 212, 15, "CX-60", "CX-60", "cx-60" },
                    { 213, 15, "CX-70", "CX-70", "cx-70" },
                    { 214, 15, "CX-9", "CX-9", "cx-9" },
                    { 215, 15, "CX-90", "CX-90", "cx-90" },
                    { 216, 15, "MX-5", "MX-5", "mx-5" },
                    { 217, 16, "Attrage", "Attrage", "attrage" },
                    { 218, 16, "Mirage", "Mirage", "mirage" },
                    { 219, 16, "Lancer", "Lancer", "lancer" },
                    { 220, 16, "ASX", "ASX", "asx" },
                    { 221, 16, "Eclipse Cross", "Eclipse Cross", "eclipse-cross" },
                    { 222, 16, "Outlander", "Outlander", "outlander" },
                    { 223, 16, "Pajero", "Pajero", "pajero" },
                    { 224, 16, "Pajero Sport", "Pajero Sport", "pajero-sport" },
                    { 225, 16, "Eclipse", "Eclipse", "eclipse" },
                    { 226, 17, "Impreza", "Impreza", "impreza" },
                    { 227, 17, "Legacy", "Legacy", "legacy" },
                    { 228, 17, "Outback", "Outback", "outback" },
                    { 229, 17, "Forester", "Forester", "forester" },
                    { 230, 17, "Crosstrek", "Crosstrek", "crosstrek" },
                    { 231, 17, "Ascent", "Ascent", "ascent" },
                    { 232, 17, "BRZ", "BRZ", "brz" },
                    { 233, 17, "WRX", "WRX", "wrx" },
                    { 234, 17, "Solterra", "Solterra", "solterra" },
                    { 235, 18, "Swift", "Swift", "swift" },
                    { 236, 18, "Baleno", "Baleno", "baleno" },
                    { 237, 18, "Ciaz", "Ciaz", "ciaz" },
                    { 238, 18, "Dzire", "Dzire", "dzire" },
                    { 239, 18, "Vitara", "Vitara", "vitara" },
                    { 240, 18, "Grand Vitara", "Grand Vitara", "grand-vitara" },
                    { 241, 18, "S-Cross", "S-Cross", "s-cross" },
                    { 242, 18, "Jimny", "Jimny", "jimny" },
                    { 243, 18, "Ignis", "Ignis", "ignis" },
                    { 244, 18, "SX4", "SX4", "sx4" },
                    { 245, 19, "Q30", "Q30", "q30" },
                    { 246, 19, "Q50", "Q50", "q50" },
                    { 247, 19, "Q60", "Q60", "q60" },
                    { 248, 19, "Q70", "Q70", "q70" },
                    { 249, 19, "QX30", "QX30", "qx30" },
                    { 250, 19, "QX50", "QX50", "qx50" },
                    { 251, 19, "QX55", "QX55", "qx55" },
                    { 252, 19, "QX60", "QX60", "qx60" },
                    { 253, 19, "QX70", "QX70", "qx70" },
                    { 254, 19, "QX80", "QX80", "qx80" },
                    { 255, 20, "Clio", "Clio", "clio" },
                    { 256, 20, "Megane", "Megane", "megane" },
                    { 257, 20, "Logan", "Logan", "logan" },
                    { 258, 20, "Sandero", "Sandero", "sandero" },
                    { 259, 20, "Duster", "Duster", "duster" },
                    { 260, 20, "Captur", "Captur", "captur" },
                    { 261, 20, "Kaptur", "Kaptur", "kaptur" },
                    { 262, 20, "Arkana", "Arkana", "arkana" },
                    { 263, 20, "Koleos", "Koleos", "koleos" },
                    { 264, 20, "Austral", "Austral", "austral" },
                    { 265, 20, "Talisman", "Talisman", "talisman" },
                    { 266, 20, "Kwid", "Kwid", "kwid" },
                    { 267, 21, "108", "108", "108" },
                    { 268, 21, "208", "208", "208" },
                    { 269, 21, "2008", "2008", "2008" },
                    { 270, 21, "308", "308", "308" },
                    { 271, 21, "3008", "3008", "3008" },
                    { 272, 21, "408", "408", "408" },
                    { 273, 21, "508", "508", "508" },
                    { 274, 21, "5008", "5008", "5008" },
                    { 275, 21, "Rifter", "Rifter", "rifter" },
                    { 276, 21, "Traveller", "Traveller", "traveller" },
                    { 277, 22, "C1", "C1", "c1" },
                    { 278, 22, "C3", "C3", "c3" },
                    { 279, 22, "C3 Aircross", "C3 Aircross", "c3-aircross" },
                    { 280, 22, "C4", "C4", "c4" },
                    { 281, 22, "C4 X", "C4 X", "c4-x" },
                    { 282, 22, "C5 Aircross", "C5 Aircross", "c5-aircross" },
                    { 283, 22, "C5 X", "C5 X", "c5-x" },
                    { 284, 22, "Berlingo", "Berlingo", "berlingo" },
                    { 285, 22, "SpaceTourer", "SpaceTourer", "spacetourer" },
                    { 286, 23, "Adam", "Adam", "adam" },
                    { 287, 23, "Corsa", "Corsa", "corsa" },
                    { 288, 23, "Astra", "Astra", "astra" },
                    { 289, 23, "Insignia", "Insignia", "insignia" },
                    { 290, 23, "Mokka", "Mokka", "mokka" },
                    { 291, 23, "Crossland", "Crossland", "crossland" },
                    { 292, 23, "Grandland", "Grandland", "grandland" },
                    { 293, 23, "Frontera", "Frontera", "frontera" },
                    { 294, 23, "Zafira", "Zafira", "zafira" },
                    { 295, 24, "Fabia", "Fabia", "fabia" },
                    { 296, 24, "Scala", "Scala", "scala" },
                    { 297, 24, "Octavia", "Octavia", "octavia" },
                    { 298, 24, "Superb", "Superb", "superb" },
                    { 299, 24, "Kamiq", "Kamiq", "kamiq" },
                    { 300, 24, "Karoq", "Karoq", "karoq" },
                    { 301, 24, "Kodiaq", "Kodiaq", "kodiaq" },
                    { 302, 24, "Enyaq", "Enyaq", "enyaq" },
                    { 303, 24, "Kushaq", "Kushaq", "kushaq" },
                    { 304, 24, "Slavia", "Slavia", "slavia" },
                    { 305, 25, "S60", "S60", "s60" },
                    { 306, 25, "S90", "S90", "s90" },
                    { 307, 25, "V40", "V40", "v40" },
                    { 308, 25, "V60", "V60", "v60" },
                    { 309, 25, "V90", "V90", "v90" },
                    { 310, 25, "XC40", "XC40", "xc40" },
                    { 311, 25, "XC60", "XC60", "xc60" },
                    { 312, 25, "XC90", "XC90", "xc90" },
                    { 313, 25, "EX30", "EX30", "ex30" },
                    { 314, 25, "EX40", "EX40", "ex40" },
                    { 315, 25, "EX90", "EX90", "ex90" },
                    { 316, 26, "XE", "XE", "xe" },
                    { 317, 26, "XF", "XF", "xf" },
                    { 318, 26, "XJ", "XJ", "xj" },
                    { 319, 26, "F-Pace", "F-Pace", "f-pace" },
                    { 320, 26, "E-Pace", "E-Pace", "e-pace" },
                    { 321, 26, "I-Pace", "I-Pace", "i-pace" },
                    { 322, 26, "F-Type", "F-Type", "f-type" },
                    { 323, 27, "Renegade", "Renegade", "renegade" },
                    { 324, 27, "Compass", "Compass", "compass" },
                    { 325, 27, "Cherokee", "Cherokee", "cherokee" },
                    { 326, 27, "Grand Cherokee", "Grand Cherokee", "grand-cherokee" },
                    { 327, 27, "Wrangler", "Wrangler", "wrangler" },
                    { 328, 27, "Gladiator", "Gladiator", "gladiator" },
                    { 329, 27, "Avenger", "Avenger", "avenger" },
                    { 330, 27, "Commander", "Commander", "commander" },
                    { 331, 28, "Model S", "Model S", "model-s" },
                    { 332, 28, "Model 3", "Model 3", "model-3" },
                    { 333, 28, "Model X", "Model X", "model-x" },
                    { 334, 28, "Model Y", "Model Y", "model-y" },
                    { 335, 28, "Cybertruck", "Cybertruck", "cybertruck" },
                    { 336, 28, "Roadster", "Roadster", "roadster" },
                    { 337, 29, "Atto 3", "Atto 3", "atto-3" },
                    { 338, 29, "Dolphin", "Dolphin", "dolphin" },
                    { 339, 29, "Seal", "Seal", "seal" },
                    { 340, 29, "Seal U", "Seal U", "seal-u" },
                    { 341, 29, "Han", "Han", "han" },
                    { 342, 29, "Tang", "Tang", "tang" },
                    { 343, 29, "Song Plus", "Song Plus", "song-plus" },
                    { 344, 29, "Song Pro", "Song Pro", "song-pro" },
                    { 345, 29, "Qin Plus", "Qin Plus", "qin-plus" },
                    { 346, 29, "Yuan Plus", "Yuan Plus", "yuan-plus" },
                    { 347, 29, "Seagull", "Seagull", "seagull" },
                    { 348, 29, "Leopard 5", "Leopard 5", "leopard-5" },
                    { 349, 30, "Tiggo 2", "Tiggo 2", "tiggo-2" },
                    { 350, 30, "Tiggo 4", "Tiggo 4", "tiggo-4" },
                    { 351, 30, "Tiggo 7", "Tiggo 7", "tiggo-7" },
                    { 352, 30, "Tiggo 8", "Tiggo 8", "tiggo-8" },
                    { 353, 30, "Tiggo 9", "Tiggo 9", "tiggo-9" },
                    { 354, 30, "Arrizo 5", "Arrizo 5", "arrizo-5" },
                    { 355, 30, "Arrizo 6", "Arrizo 6", "arrizo-6" },
                    { 356, 30, "Arrizo 8", "Arrizo 8", "arrizo-8" },
                    { 357, 31, "Emgrand", "Emgrand", "emgrand" },
                    { 358, 31, "Coolray", "Coolray", "coolray" },
                    { 359, 31, "Atlas", "Atlas", "atlas" },
                    { 360, 31, "Monjaro", "Monjaro", "monjaro" },
                    { 361, 31, "Tugella", "Tugella", "tugella" },
                    { 362, 31, "Okavango", "Okavango", "okavango" },
                    { 363, 31, "Preface", "Preface", "preface" },
                    { 364, 31, "Starray", "Starray", "starray" },
                    { 365, 31, "EX5", "EX5", "ex5" },
                    { 366, 31, "Galaxy E5", "Galaxy E5", "galaxy-e5" },
                    { 367, 31, "Galaxy L7", "Galaxy L7", "galaxy-l7" },
                    { 368, 32, "Jolion", "Jolion", "jolion" },
                    { 369, 32, "H6", "H6", "h6" },
                    { 370, 32, "H9", "H9", "h9" },
                    { 371, 32, "Dargo", "Dargo", "dargo" },
                    { 372, 32, "F7", "F7", "f7" },
                    { 373, 32, "F7x", "F7x", "f7x" },
                    { 374, 32, "M6", "M6", "m6" },
                    { 375, 32, "H5", "H5", "h5" },
                    { 376, 33, "Alsvin", "Alsvin", "alsvin" },
                    { 377, 33, "Eado", "Eado", "eado" },
                    { 378, 33, "CS35 Plus", "CS35 Plus", "cs35-plus" },
                    { 379, 33, "CS55 Plus", "CS55 Plus", "cs55-plus" },
                    { 380, 33, "CS75 Plus", "CS75 Plus", "cs75-plus" },
                    { 381, 33, "CS95", "CS95", "cs95" },
                    { 382, 33, "UNI-T", "UNI-T", "uni-t" },
                    { 383, 33, "UNI-K", "UNI-K", "uni-k" },
                    { 384, 33, "UNI-V", "UNI-V", "uni-v" },
                    { 385, 33, "Deepal S07", "Deepal S07", "deepal-s07" },
                    { 386, 33, "Deepal S05", "Deepal S05", "deepal-s05" },
                    { 387, 33, "Lumin", "Lumin", "lumin" },
                    { 388, 34, "GS3", "GS3", "gs3" },
                    { 389, 34, "GS4", "GS4", "gs4" },
                    { 390, 34, "GS5", "GS5", "gs5" },
                    { 391, 34, "GS8", "GS8", "gs8" },
                    { 392, 34, "Empow", "Empow", "empow" },
                    { 393, 34, "Emkoo", "Emkoo", "emkoo" },
                    { 394, 34, "Aion Y", "Aion Y", "aion-y" },
                    { 395, 34, "Aion V", "Aion V", "aion-v" },
                    { 396, 34, "Hyptec HT", "Hyptec HT", "hyptec-ht" },
                    { 397, 35, "JS3", "JS3", "js3" },
                    { 398, 35, "JS4", "JS4", "js4" },
                    { 399, 35, "JS6", "JS6", "js6" },
                    { 400, 35, "J7", "J7", "j7" },
                    { 401, 35, "S3", "S3", "s3" },
                    { 402, 35, "S5", "S5", "s5" },
                    { 403, 35, "E30X", "E30X", "e30x" },
                    { 404, 35, "RF8", "RF8", "rf8" },
                    { 405, 36, "X50", "X50", "x50" },
                    { 406, 36, "X70", "X70", "x70" },
                    { 407, 36, "X70 Plus", "X70 Plus", "x70-plus" },
                    { 408, 36, "X90 Plus", "X90 Plus", "x90-plus" },
                    { 409, 36, "Dashing", "Dashing", "dashing" },
                    { 410, 36, "T1", "T1", "t1" },
                    { 411, 36, "T2", "T2", "t2" },
                    { 412, 36, "Traveller", "Traveller", "traveller" },
                    { 413, 37, "LX", "LX", "lx" },
                    { 414, 37, "TXL", "TXL", "txl" },
                    { 415, 37, "VX", "VX", "vx" },
                    { 416, 37, "RX", "RX", "rx" },
                    { 417, 37, "ES", "ES", "es" },
                    { 418, 37, "ET", "ET", "et" },
                    { 419, 38, "H5", "H5", "h5" },
                    { 420, 38, "H6", "H6", "h6" },
                    { 421, 38, "H9", "H9", "h9" },
                    { 422, 38, "HS3", "HS3", "hs3" },
                    { 423, 38, "HS5", "HS5", "hs5" },
                    { 424, 38, "HS7", "HS7", "hs7" },
                    { 425, 38, "E-HS9", "E-HS9", "e-hs9" },
                    { 426, 38, "E-QM5", "E-QM5", "e-qm5" },
                    { 427, 38, "EH7", "EH7", "eh7" },
                    { 428, 39, "Huge", "Huge", "huge" },
                    { 429, 39, "T5 Evo", "T5 Evo", "t5-evo" },
                    { 430, 39, "Shine", "Shine", "shine" },
                    { 431, 39, "Fengon 500", "Fengon 500", "fengon-500" },
                    { 432, 39, "Fengon 580", "Fengon 580", "fengon-580" },
                    { 433, 39, "Forthing T5", "Forthing T5", "forthing-t5" },
                    { 434, 40, "Bestune T77", "Bestune T77", "bestune-t77" },
                    { 435, 40, "Bestune T99", "Bestune T99", "bestune-t99" },
                    { 436, 40, "Bestune B70", "Bestune B70", "bestune-b70" },
                    { 437, 40, "Bestune T55", "Bestune T55", "bestune-t55" },
                    { 438, 40, "NAT", "NAT", "nat" },
                    { 439, 41, "L6", "L6", "l6" },
                    { 440, 41, "L7", "L7", "l7" },
                    { 441, 41, "L8", "L8", "l8" },
                    { 442, 41, "L9", "L9", "l9" },
                    { 443, 41, "MEGA", "MEGA", "mega" },
                    { 444, 42, "001", "001", "001" },
                    { 445, 42, "007", "007", "007" },
                    { 446, 42, "009", "009", "009" },
                    { 447, 42, "X", "X", "x" },
                    { 448, 42, "7X", "7X", "7x" },
                    { 449, 42, "MIX", "MIX", "mix" },
                    { 450, 43, "G3", "G3", "g3" },
                    { 451, 43, "G6", "G6", "g6" },
                    { 452, 43, "G9", "G9", "g9" },
                    { 453, 43, "P5", "P5", "p5" },
                    { 454, 43, "P7", "P7", "p7" },
                    { 455, 43, "Mona M03", "Mona M03", "mona-m03" },
                    { 456, 43, "X9", "X9", "x9" },
                    { 457, 44, "C5", "C5", "c5" },
                    { 458, 44, "C7", "C7", "c7" },
                    { 459, 44, "E5", "E5", "e5" },
                    { 460, 44, "C9", "C9", "c9" },
                    { 461, 45, "J5", "J5", "j5" },
                    { 462, 45, "J7", "J7", "j7" },
                    { 463, 45, "J8", "J8", "j8" },
                    { 464, 46, "300", "300", "300" },
                    { 465, 46, "400", "400", "400" },
                    { 466, 46, "500", "500", "500" },
                    { 467, 46, "700", "700", "700" },
                    { 468, 47, "MG 3", "MG 3", "mg-3" },
                    { 469, 47, "MG 4", "MG 4", "mg-4" },
                    { 470, 47, "MG 5", "MG 5", "mg-5" },
                    { 471, 47, "MG 6", "MG 6", "mg-6" },
                    { 472, 47, "MG 7", "MG 7", "mg-7" },
                    { 473, 47, "ZS", "ZS", "zs" },
                    { 474, 47, "HS", "HS", "hs" },
                    { 475, 47, "RX5", "RX5", "rx5" },
                    { 476, 47, "Marvel R", "Marvel R", "marvel-r" },
                    { 477, 47, "Cyberster", "Cyberster", "cyberster" },
                    { 478, 48, "H6", "H6", "h6" },
                    { 479, 48, "Poer", "Poer", "poer" },
                    { 480, 48, "Wingle", "Wingle", "wingle" },
                    { 481, 48, "Cannon", "Cannon", "cannon" },
                    { 482, 48, "Hover", "Hover", "hover" },
                    { 483, 49, "Granta", "Granta", "granta" },
                    { 484, 49, "Vesta", "Vesta", "vesta" },
                    { 485, 49, "Niva Legend", "Niva Legend", "niva-legend" },
                    { 486, 49, "Niva Travel", "Niva Travel", "niva-travel" },
                    { 487, 49, "Largus", "Largus", "largus" },
                    { 488, 49, "XRAY", "XRAY", "xray" },
                    { 489, 49, "Priora", "Priora", "priora" },
                    { 490, 49, "Kalina", "Kalina", "kalina" },
                    { 491, 50, "X35", "X35", "x35" },
                    { 492, 50, "X55", "X55", "x55" },
                    { 493, 50, "X7", "X7", "x7" },
                    { 494, 50, "BJ30", "BJ30", "bj30" },
                    { 495, 50, "BJ40", "BJ40", "bj40" },
                    { 496, 50, "BJ60", "BJ60", "bj60" },
                    { 497, 50, "EU5", "EU5", "eu5" },
                    { 498, 50, "U5 Plus", "U5 Plus", "u5-plus" },
                    { 499, 51, "T03", "T03", "t03" },
                    { 500, 51, "C10", "C10", "c10" },
                    { 501, 51, "C11", "C11", "c11" },
                    { 502, 51, "C16", "C16", "c16" },
                    { 503, 51, "B10", "B10", "b10" },
                    { 504, 52, "ET5", "ET5", "et5" },
                    { 505, 52, "ET7", "ET7", "et7" },
                    { 506, 52, "ET9", "ET9", "et9" },
                    { 507, 52, "ES6", "ES6", "es6" },
                    { 508, 52, "ES7", "ES7", "es7" },
                    { 509, 52, "ES8", "ES8", "es8" },
                    { 510, 52, "EC6", "EC6", "ec6" },
                    { 511, 52, "EC7", "EC7", "ec7" },
                    { 512, 52, "EL6", "EL6", "el6" },
                    { 513, 52, "EL7", "EL7", "el7" },
                    { 514, 52, "EL8", "EL8", "el8" },
                    { 515, 53, "Free", "Free", "free" },
                    { 516, 53, "Dream", "Dream", "dream" },
                    { 517, 53, "Passion", "Passion", "passion" },
                    { 518, 53, "Courage", "Courage", "courage" },
                    { 519, 54, "Seres 3", "Seres 3", "seres-3" },
                    { 520, 54, "Seres 5", "Seres 5", "seres-5" },
                    { 521, 54, "Seres 7", "Seres 7", "seres-7" },
                    { 522, 54, "Aito M5", "Aito M5", "aito-m5" },
                    { 523, 54, "Aito M7", "Aito M7", "aito-m7" },
                    { 524, 54, "Aito M9", "Aito M9", "aito-m9" },
                    { 525, 55, "Logan", "Logan", "logan" },
                    { 526, 55, "Sandero", "Sandero", "sandero" },
                    { 527, 55, "Duster", "Duster", "duster" },
                    { 528, 55, "Jogger", "Jogger", "jogger" },
                    { 529, 55, "Spring", "Spring", "spring" },
                    { 530, 55, "Bigster", "Bigster", "bigster" },
                    { 531, 56, "500", "500", "500" },
                    { 532, 56, "500X", "500X", "500x" },
                    { 533, 56, "Panda", "Panda", "panda" },
                    { 534, 56, "Tipo", "Tipo", "tipo" },
                    { 535, 56, "Punto", "Punto", "punto" },
                    { 536, 56, "Doblo", "Doblo", "doblo" },
                    { 537, 56, "600", "600", "600" },
                    { 538, 56, "Grande Panda", "Grande Panda", "grande-panda" },
                    { 539, 57, "Giulia", "Giulia", "giulia" },
                    { 540, 57, "Stelvio", "Stelvio", "stelvio" },
                    { 541, 57, "Tonale", "Tonale", "tonale" },
                    { 542, 57, "Junior", "Junior", "junior" },
                    { 543, 57, "Giulietta", "Giulietta", "giulietta" },
                    { 544, 57, "159", "159", "159" },
                    { 545, 58, "Bentayga", "Bentayga", "bentayga" },
                    { 546, 58, "Continental GT", "Continental GT", "continental-gt" },
                    { 547, 58, "Flying Spur", "Flying Spur", "flying-spur" },
                    { 548, 58, "Continental GTC", "Continental GTC", "continental-gtc" },
                    { 549, 58, "Mulsanne", "Mulsanne", "mulsanne" },
                    { 550, 59, "Cullinan", "Cullinan", "cullinan" },
                    { 551, 59, "Ghost", "Ghost", "ghost" },
                    { 552, 59, "Phantom", "Phantom", "phantom" },
                    { 553, 59, "Spectre", "Spectre", "spectre" },
                    { 554, 59, "Wraith", "Wraith", "wraith" },
                    { 555, 59, "Dawn", "Dawn", "dawn" },
                    { 556, 60, "Ghibli", "Ghibli", "ghibli" },
                    { 557, 60, "Quattroporte", "Quattroporte", "quattroporte" },
                    { 558, 60, "Levante", "Levante", "levante" },
                    { 559, 60, "Grecale", "Grecale", "grecale" },
                    { 560, 60, "MC20", "MC20", "mc20" },
                    { 561, 60, "GranTurismo", "GranTurismo", "granturismo" },
                    { 562, 60, "GranCabrio", "GranCabrio", "grancabrio" },
                    { 563, 61, "Integra", "Integra", "integra" },
                    { 564, 61, "TLX", "TLX", "tlx" },
                    { 565, 61, "ILX", "ILX", "ilx" },
                    { 566, 61, "RLX", "RLX", "rlx" },
                    { 567, 61, "MDX", "MDX", "mdx" },
                    { 568, 61, "RDX", "RDX", "rdx" },
                    { 569, 61, "ZDX", "ZDX", "zdx" },
                    { 570, 61, "NSX", "NSX", "nsx" },
                    { 571, 62, "Navigator", "Navigator", "navigator" },
                    { 572, 62, "Aviator", "Aviator", "aviator" },
                    { 573, 62, "Nautilus", "Nautilus", "nautilus" },
                    { 574, 62, "Corsair", "Corsair", "corsair" },
                    { 575, 62, "Continental", "Continental", "continental" },
                    { 576, 62, "MKZ", "MKZ", "mkz" },
                    { 577, 63, "Charger", "Charger", "charger" },
                    { 578, 63, "Challenger", "Challenger", "challenger" },
                    { 579, 63, "Durango", "Durango", "durango" },
                    { 580, 63, "Journey", "Journey", "journey" },
                    { 581, 63, "Hornet", "Hornet", "hornet" },
                    { 582, 63, "Viper", "Viper", "viper" },
                    { 583, 64, "Ibiza", "Ibiza", "ibiza" },
                    { 584, 64, "Leon", "Leon", "leon" },
                    { 585, 64, "Arona", "Arona", "arona" },
                    { 586, 64, "Ateca", "Ateca", "ateca" },
                    { 587, 64, "Tarraco", "Tarraco", "tarraco" },
                    { 588, 65, "ForTwo", "ForTwo", "fortwo" },
                    { 589, 65, "ForFour", "ForFour", "forfour" },
                    { 590, 65, "Roadster", "Roadster", "roadster" },
                    { 591, 65, "#1", "#1", "1" },
                    { 592, 65, "#3", "#3", "3" },
                    { 593, 66, "Tivoli", "Tivoli", "tivoli" },
                    { 594, 66, "Korando", "Korando", "korando" },
                    { 595, 66, "Rexton", "Rexton", "rexton" },
                    { 596, 66, "Torres", "Torres", "torres" },
                    { 597, 66, "Musso", "Musso", "musso" },
                    { 598, 66, "Actyon", "Actyon", "actyon" },
                    { 599, 67, "01", "01", "01" },
                    { 600, 67, "02", "02", "02" },
                    { 601, 67, "03", "03", "03" },
                    { 602, 67, "05", "05", "05" },
                    { 603, 67, "06", "06", "06" },
                    { 604, 67, "08", "08", "08" },
                    { 605, 67, "09", "09", "09" },
                    { 606, 68, "Matiz", "Matiz", "matiz" },
                    { 607, 68, "Nexia", "Nexia", "nexia" },
                    { 608, 68, "Lanos", "Lanos", "lanos" },
                    { 609, 68, "Leganza", "Leganza", "leganza" },
                    { 610, 68, "Lacetti", "Lacetti", "lacetti" },
                    { 611, 68, "Magnus", "Magnus", "magnus" },
                    { 612, 68, "Tosca", "Tosca", "tosca" },
                    { 613, 68, "Winstorm", "Winstorm", "winstorm" },
                    { 614, 69, "Terios", "Terios", "terios" },
                    { 615, 69, "Sirion", "Sirion", "sirion" },
                    { 616, 69, "Mira", "Mira", "mira" },
                    { 617, 69, "Rocky", "Rocky", "rocky" },
                    { 618, 69, "Copen", "Copen", "copen" },
                    { 619, 69, "Move", "Move", "move" },
                    { 620, 69, "Tanto", "Tanto", "tanto" },
                    { 621, 70, "Envision", "Envision", "envision" },
                    { 622, 70, "Enclave", "Enclave", "enclave" },
                    { 623, 70, "Encore", "Encore", "encore" },
                    { 624, 70, "Encore GX", "Encore GX", "encore-gx" },
                    { 625, 70, "Envista", "Envista", "envista" },
                    { 626, 70, "Electra E5", "Electra E5", "electra-e5" },
                    { 627, 71, "Terrain", "Terrain", "terrain" },
                    { 628, 71, "Acadia", "Acadia", "acadia" },
                    { 629, 71, "Yukon", "Yukon", "yukon" },
                    { 630, 71, "Canyon", "Canyon", "canyon" },
                    { 631, 71, "Sierra", "Sierra", "sierra" },
                    { 632, 71, "Hummer EV", "Hummer EV", "hummer-ev" },
                    { 633, 72, "D-Max", "D-Max", "d-max" },
                    { 634, 72, "MU-X", "MU-X", "mu-x" },
                    { 635, 72, "MU-7", "MU-7", "mu-7" },
                    { 636, 73, "300", "300", "300" },
                    { 637, 73, "Pacifica", "Pacifica", "pacifica" },
                    { 638, 73, "Voyager", "Voyager", "voyager" },
                    { 639, 73, "Town & Country", "Town & Country", "town-and-country" },
                    { 640, 73, "200", "200", "200" },
                    { 641, 74, "Formentor", "Formentor", "formentor" },
                    { 642, 74, "Born", "Born", "born" },
                    { 643, 74, "Leon", "Leon", "leon" },
                    { 644, 74, "Ateca", "Ateca", "ateca" },
                    { 645, 74, "Terramar", "Terramar", "terramar" },
                    { 646, 74, "Tavascan", "Tavascan", "tavascan" },
                    { 647, 75, "1500", "1500", "1500" },
                    { 648, 75, "2500", "2500", "2500" },
                    { 649, 75, "3500", "3500", "3500" },
                    { 650, 75, "ProMaster", "ProMaster", "promaster" },
                    { 651, 75, "Rampage", "Rampage", "rampage" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 88);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 89);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 90);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 91);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 92);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 93);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 94);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 95);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 96);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 98);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 113);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 114);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 115);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 117);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 118);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 119);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 120);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 124);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 125);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 126);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 127);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 128);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 129);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 130);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 131);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 132);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 133);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 134);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 135);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 136);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 137);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 138);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 139);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 140);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 141);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 142);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 143);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 144);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 145);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 146);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 147);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 148);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 149);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 150);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 151);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 152);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 153);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 154);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 155);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 156);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 157);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 158);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 159);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 160);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 161);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 162);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 163);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 164);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 165);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 166);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 167);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 168);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 169);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 170);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 171);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 172);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 173);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 174);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 175);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 176);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 177);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 178);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 179);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 180);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 181);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 182);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 183);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 184);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 185);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 186);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 187);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 188);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 189);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 190);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 191);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 192);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 193);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 194);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 195);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 196);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 197);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 198);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 199);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 200);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 201);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 202);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 203);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 204);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 205);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 206);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 207);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 208);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 209);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 210);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 211);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 212);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 213);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 214);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 215);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 216);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 217);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 218);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 219);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 220);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 221);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 222);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 223);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 224);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 225);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 226);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 227);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 228);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 229);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 230);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 231);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 232);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 233);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 234);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 235);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 236);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 237);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 238);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 239);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 240);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 241);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 242);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 243);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 244);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 245);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 246);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 247);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 248);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 249);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 250);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 251);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 252);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 253);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 254);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 255);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 256);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 257);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 258);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 259);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 260);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 261);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 262);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 263);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 264);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 265);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 266);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 267);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 268);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 269);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 270);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 271);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 272);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 273);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 274);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 275);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 276);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 277);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 278);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 279);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 280);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 281);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 282);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 283);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 284);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 285);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 286);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 287);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 288);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 289);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 290);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 291);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 292);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 293);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 294);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 295);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 296);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 297);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 298);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 299);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 300);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 301);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 302);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 303);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 304);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 305);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 306);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 307);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 308);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 309);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 310);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 311);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 312);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 313);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 314);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 315);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 316);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 317);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 318);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 319);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 320);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 321);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 322);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 323);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 324);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 325);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 326);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 327);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 328);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 329);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 330);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 331);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 332);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 333);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 334);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 335);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 336);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 337);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 338);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 339);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 340);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 341);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 342);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 343);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 344);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 345);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 346);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 347);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 348);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 349);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 350);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 351);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 352);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 353);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 354);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 355);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 356);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 357);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 358);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 359);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 360);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 361);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 362);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 363);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 364);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 365);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 366);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 367);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 368);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 369);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 370);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 371);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 372);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 373);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 374);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 375);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 376);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 377);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 378);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 379);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 380);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 381);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 382);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 383);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 384);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 385);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 386);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 387);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 388);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 389);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 390);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 391);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 392);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 393);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 394);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 395);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 396);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 397);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 398);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 399);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 400);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 401);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 402);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 403);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 404);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 405);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 406);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 407);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 408);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 409);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 410);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 411);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 412);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 413);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 414);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 415);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 416);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 417);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 418);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 419);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 420);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 421);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 422);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 423);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 424);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 425);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 426);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 427);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 428);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 429);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 430);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 431);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 432);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 433);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 434);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 435);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 436);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 437);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 438);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 439);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 440);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 441);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 442);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 443);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 444);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 445);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 446);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 447);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 448);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 449);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 450);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 451);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 452);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 453);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 454);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 455);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 456);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 457);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 458);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 459);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 460);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 461);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 462);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 463);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 464);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 465);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 466);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 467);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 468);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 469);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 470);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 471);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 472);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 473);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 474);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 475);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 476);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 477);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 478);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 479);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 480);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 481);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 482);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 483);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 484);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 485);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 486);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 487);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 488);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 489);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 490);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 491);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 492);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 493);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 494);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 495);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 496);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 497);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 498);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 499);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 500);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 501);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 502);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 503);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 504);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 505);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 506);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 507);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 508);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 509);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 510);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 511);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 512);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 513);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 514);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 515);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 516);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 517);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 518);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 519);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 520);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 521);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 522);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 523);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 524);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 525);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 526);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 527);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 528);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 529);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 530);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 531);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 532);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 533);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 534);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 535);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 536);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 537);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 538);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 539);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 540);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 541);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 542);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 543);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 544);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 545);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 546);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 547);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 548);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 549);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 550);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 551);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 552);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 553);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 554);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 555);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 556);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 557);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 558);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 559);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 560);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 561);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 562);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 563);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 564);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 565);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 566);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 567);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 568);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 569);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 570);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 571);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 572);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 573);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 574);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 575);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 576);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 577);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 578);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 579);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 580);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 581);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 582);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 583);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 584);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 585);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 586);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 587);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 588);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 589);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 590);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 591);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 592);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 593);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 594);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 595);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 596);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 597);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 598);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 599);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 600);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 601);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 602);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 603);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 604);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 605);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 606);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 607);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 608);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 609);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 610);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 611);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 612);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 613);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 614);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 615);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 616);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 617);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 618);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 619);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 620);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 621);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 622);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 623);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 624);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 625);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 626);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 627);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 628);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 629);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 630);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 631);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 632);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 633);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 634);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 635);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 636);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 637);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 638);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 639);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 640);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 641);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 642);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 643);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 644);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 645);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 646);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 647);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 648);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 649);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 650);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_models",
                keyColumn: "Id",
                keyValue: 651);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                schema: "catalog",
                table: "vehicle_makes",
                keyColumn: "Id",
                keyValue: 75);
        }
    }
}
