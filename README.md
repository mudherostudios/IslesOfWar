# Isles of War

Use Unity version 2018.3.10f1. Don't update.

- [Realtime Board Ideas - Called Miro Now](https://miro.com/app/board/o9J_kyO1Uzc=/)


# !IMPORTANT!

There is problem with unity package updating, (not assets from the store, from the package manager). I believe the packages are housed in library, but library is ignored in the gitignore file. It is suggested by the community to keep it ignored. I'm not familiar enough with the Library folder and the inner workings of unity to know what is needed or not so I am just not going to touch it until I can mess with it on a new project when I have time. So for this project if someone updates a package under package manager and pushes it, the manifest will update on github but the packages will not. That means every user will need to update their packages manually before pulling the updated manifest. It's easy to do, but also easy to forget, so just leaving a note here.

Also, if you pull it fresh, you will need to change all of the versions in the manifest to 1.0.0 or a lower version than what it is at. When you pull it as a fresh clone, unity will likely give you a bunch of errors saying the package is missing. Which it will be missing because the name of the folder will be named after the version number. So just revert the version number to a lower one and unity will know there is a something wrong but still show them to you. Then go to the Package Manager and update all of the non-1.0.0 packages that were in the manifest. They should also be listed in the errors in the console. That should fix everything.
