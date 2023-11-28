## Documentation

__Always prefer Harmony's [official documentation](https://chsakell.gitbook.io/harmony/) which always contain the latest docs.__

### Set database connection string
Open __appsettings.json__ and set the connection string to point to your SQL Server instance
```javascript
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=Harmony;Integrated Security=True;TrustServerCertificate=True"
  },
```

### Database migrations
By default, when you start the application, the app will try to run all migrations to your database. You can of course disable this for production environment. You can also run migrations using the command line. Check the __migrations.pdf__ for sample commands.

### Start application
Set the `Harmony.Server` project as the startup and fire up the application

### Default users created
By default two users will be created for you:
1. username: __administrator__ , password: __Pa$$w0rd!__
2. username: __johndoe__ , password: __Pa$$w0rd!__

You can use both of them but you can register your own of course. Make sure you change the administrator's password in a production environment from the account page.

### Register account

Click `Register` and fill all the required fields
![Register user](./images/harmony_register_user_22.png)

### Login
Click `Sign In` and enter your credentials
![Login user](./images/harmony_login_user_21.png)

### Create a workspace
From the upper right click `CREATE` and select `Create Workspace`. Or you can click the `START NOW` button on the Home page
![Create workspace](./images/harmony_create_workspace_2.png)

### Add members to a workspace
With an __administrator__ account click the `Members` menu item on the left menu. Search and add/remove members to the workspace
![Add workspace members](./images/harmony_workspace_add_member_19.png)


### Add a board or a scrum project
From the upper right click `CREATE` and select `Create Board`. Or you can click the `START NOW` button on the empty's workspace page
![Empty workspace](./images/harmony-empty-workspace_3.png)
![Create board](./images/harmony_create_board_4.png)
Select the board's workspace and click `Create`.

### Add members to a board
Click the `SAHRE BOARD` button on the top right of the board's top bar.
![Add board members](./images/harmony_share_board_10.png)

### Add board lists to your board
Customize your own board by creating a few lists, e.g _TODO_, _IN PROGRESS_, _COMPLETE_. Click `CREATE LIST` from the top bar in the board's page or the `START NOW` button on an empty board.
![Empty board](./images/harmony_empty_board_5.png)
![Add board list](./images/harmony_create_board_list_6.png)

### Add cards to your board
Click the __+__ button at the top of a board list and enter the card's title.
![Add card](./images/harmony_create_card_8.png)

### View card
Simply click on a card to open its contents. In the card's view you can:

1. Add set a description
![Add card](./images/harmony_view_card_13_dark.png)

2. Set multiple labels or even create new ones
![Add card labels](./images/harmony_card_set_labels_16.png)

3. Set start/due dates
![Add card labels](./images/harmony_card_add_start_due_dates_18.png)

4. Create check lists
![Create check list](./images/harmony_card_create_check_list_17.png)

5. Add check list items to check lists
![Add check list items](./images/harmony_create_check_list_item_23.png)
You can also set a due date for each item

6. Assign member(s) to a card
Click the `MEMBERS` button on the right and add members to the card
![Add card members](./images/harmony_card_assign_members_15.png)

### Account
Selec `Account` from the right top bar. Update user's details and set a profile picture for your account
![Account details](./images/harmony_account_profile_24.png)

### Dark/Light theme
Toggle the site's theme using the upper right button
![Light theme](./images/harmony_full_board_12_light.png)