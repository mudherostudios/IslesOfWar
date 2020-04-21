using Newtonsoft.Json;

namespace IslesOfWar
{
    public static class SquadConstants
    {
        //Random names for saving squads in playerPrefs. Does not affect gameStateProcessor logic.
        [JsonIgnore]
        public static string[] randomSquadNames = new string[]
        {
            "Aardvark", "Aardwolf", "Albatross", "Alligator", "Alpaca", "Anaconda", "Angelfish", "Anglerfish", "Ant", "Anteater", "Antelope",
            "Antlion", "Ape", "Aphid", "Arctic Fox", "Arctic Wolf", "Armadillo", "Baboon", "Badger", "Bandicoot", "Barnacle", "Barracuda",
            "Basilisk", "Bass", "Bat", "Bear", "Beaver", "Bedbug", "Bee", "Beetle", "Bison", "Black Widow", "Black Panther", "Blackbird",
            "Blue Bird", "Blue Jay", "Boa", "Boar", "Bobcat", "Bobolink", "Bonobo", "Buffalo", "Buzzard", "Camel", "Canary", "Capybara",
            "Cardinal", "Caribou", "Carp", "Cat", "Caterpillar", "Catfish", "Cattle", "Centipede", "Cephalopod", "Chameleon", "Cheetah",
            "Chickadee", "Chicken", "Chimpanzee", "Chinchilla", "Chipmunk", "Cicada", "Clam", "Clownfish", "Cobra", "Cockroach", "Cod", "Condor",
            "Constrictor", "Coral", "Cougar", "Cow", "Coyote", "Crab", "Crane", "Crawdad", "Crayfish", "Cricket", "Crocodile", "Crow", "Cuckoo",
            "Damselfly", "Deer", "Dingo", "Dinosaur", "Dog", "Dolphin", "Donkey", "Dormouse", "Dove", "Dragon", "Dragonfly", "Duck", "Eagle",
            "Earthworm", "Earwig", "Eel", "Egret", "Elephant", "Elk", "Emu", "Ermine", "Falcon", "Ferret", "Finch", "Firefly", "Fish", "Flamingo",
            "Flea", "Fly", "Flyingfish", "Fowl", "Fox", "Frog", "Gazelle", "Gecko", "Gerbil", "Gibbon", "Gila", "Giraffe", "Goat",
            "Goldfish", "Goose", "Gopher", "Gorilla", "Grasshopper", "Grizzly", "Grouse", "Guinea Pig", "Gull", "Guppy", "Halibut", "Hammerhead",
            "Hamster", "Hare", "Harrier", "Hawk", "Hedgehog", "Hermit", "Heron", "Herring", "Hippopotamus", "Hookworm", "Hornet", "Horse",
            "Hoverfly", "Hummingbird", "Humpback", "Hyena", "Iguana", "Impala", "Jackal", "Jaguar", "Jay", "Jellyfish", "Kangaroo", "Kingfisher",
            "Kiwi", "Koala", "Koi", "Komodo", "Krill", "Ladybug", "Lamprey", "Landfowl", "Lark", "Leech", "Lemming", "Lemur", "Leopard", "Lion",
            "Lizard", "Llama", "Lobster", "Locust", "Loon", "Louse", "Lynx", "Macaw", "Mackerel", "Magpie", "Man o' War", "Manatee", "Mandrill",
            "Manta", "Mantis", "Marlin", "Marmoset", "Marmot", "Mastodon", "Meadowlark", "Meerkat", "Mink", "Minnow", "Mite", "Mockingbird", "Mole",
            "Mollusk", "Mongoose", "Monkey", "Moose", "Mosquito", "Moth", "Mouse", "Mule", "Muskox", "Narwhal", "Newt", "Nightingale", "Ocelot",
            "Octopus", "Opossum", "Orangutan", "Orca", "Ostrich", "Otter", "Owl", "Ox", "Panda", "Panther", "Parakeet", "Parrot", "Parrotfish",
            "Partridge", "Peacock", "Pelican", "Penguin", "Perch", "Pheasant", "Pig", "Pigeon", "Pike", "Piranha", "Platypus", "Porcupine", "Porpoise",
            "Possum", "Prairie Dog", "Prawn", "Puffin", "Puma", "Python", "Quail", "Rabbit", "Raccoon", "Rat", "Rattlesnake", "Raven", "Reindeer",
            "Rhino", "Roadrunner", "Robin", "Rodent", "Rooster", "Roundworm", "Sabertooth", "Salamander", "Salmon", "Sawfish", "Scallop", "Scorpion",
            "Sea Lion", "Sea Slug", "Sea Snail", "Seahorse", "Shark", "Sheep", "Shrew", "Shrimp", "Silkworm", "Silver Fox", "Silverfish", "Skink",
            "Skunk", "Sloth", "Slug", "Snail", "Snake", "Snipe", "Sparrow", "Spider", "Spider Monkey", "Squid", "Squirrel", "Starfish", "Stingray",
            "Stork", "Sturgeon", "Swallow", "Swan", "Swordfish", "Swordtail", "Tapir", "Tarantula", "Tasmanian", "Termite", "Tick", "Tiger", "Tiger Shark",
            "Toad", "Tortoise", "Toucan", "Trout", "Tuna", "Turkey", "Turtle", "Vampire", "Viper", "Vole", "Vulture", "Wallaby", "Walrus", "Wasp",
            "Weasel", "Whale", "Whitefish", "Wildcat", "Wildebeest", "Wolf", "Wolverine", "Wombat", "Woodpecker", "Worm", "Yak", "Zebra"
        };
    }
}
