function showModalTaskPopup(storyGUID, taskGUID) {

    var popupTitle = "";

    if (taskGUID == null) {
        popupTitle = "New Task";

        // Clear the controls on the Modal popup
        clearModalTaskControls();
    }
    else {
        popupTitle = "Update Task";

        // Populate the controls on the Modal popup
        populateModalTaskControls(storyGUID, taskGUID);
    }

    // Display the modal popup
    $("#ModalTaskUpdateDialog").dialog({
        title: popupTitle,
        dialogClass: "projectGroup",
        width: 700,
        modal: true,
        buttons: {
            Update: function () {

                if (taskGUID == null) {
                    addNewTask(storyGUID);
                }
                else {
                    updateTaskValues(taskGUID);
                }

                $(this).dialog('close');
            },
            Cancel: function () {
                $(this).dialog('close');
            }
        }
    });
}

function clearModalTaskControls() {

    // Get the target objects
    var modalTaskDescription = document.getElementById("ModalTaskShortDescription");
    var modalTaskStatus = document.getElementById("ModalTaskStatusDropdown");
    var modalTaskAssignedTo = document.getElementById("ModalTaskAssignedToDropdown");
    var modalTaskHours = document.getElementById("ModalTaskHours");

    modalTaskDescription.value = "";
    modalTaskStatus.selectedIndex = 0; // Draft
    modalTaskAssignedTo.selectedIndex = 0; // No one selected
    modalTaskHours.value = "";
}

function populateModalTaskControls(storyGUID, taskGUID) {

    // Get the current values
    var taskStatus = document.getElementById("taskStatusLink" + taskGUID).innerText;
    var taskAssignedTo = document.getElementById("taskAssignedToSpan" + taskGUID).innerText;
    var taskDescription = document.getElementById("taskShortDescriptionLink" + taskGUID).innerText;
    var taskHours = document.getElementById("taskHoursSpan" + taskGUID).innerText;

    // Get the target objects
    var modalTaskShortDescription = document.getElementById("ModalTaskShortDescription");
    var modalTaskStatus = document.getElementById("ModalTaskStatusDropdown");
    var modalTaskAssignedTo = document.getElementById("ModalTaskAssignedToDropdown");
    var modalTaskHours = document.getElementById("ModalTaskHours");

    // Short Description
    modalTaskShortDescription.value = taskDescription;

    // Status
    for (var index = 0; index < modalTaskStatus.options.length; index++) {
        if (modalTaskStatus.options[index].innerHTML == taskStatus) {
            modalTaskStatus.selectedIndex = index;
            break;
        }
    }

    // Assigned To
    for (var index = 0; index < modalTaskAssignedTo.options.length; index++) {
        if (modalTaskAssignedTo.options[index].innerHTML == taskAssignedTo) {
            modalTaskAssignedTo.selectedIndex = index;
            break;
        }
    }

    // Hours
    if (taskHours == "-") {
        taskHours = "";
    }
    modalTaskHours.value = taskHours;
}

function updateTaskValues(taskGuid) {

    // Get Description
    var newDescription = document.getElementById("ModalTaskShortDescription").value;

    // Get Status
    var statusDropdown = document.getElementById("ModalTaskStatusDropdown");
    var newStatus = statusDropdown.options[statusDropdown.selectedIndex].value;

    // Get AssignedTo
    var assignedToDropdown = document.getElementById("ModalTaskAssignedToDropdown");
    var newAssignedTo = assignedToDropdown.options[assignedToDropdown.selectedIndex].value;

    // Get Hours
    var newHours = document.getElementById("ModalTaskHours").value;

    //string taskGUID, string shortDescription, string statusValue, string assignedToGUID
    $.ajax({
        type: 'POST',
        url: 'WorkList.aspx/UpdateTask',
        data: JSON.stringify({
            taskGUID: taskGuid,
            shortDescription: newDescription,
            statusValue: newStatus,
            assignedToGUID: newAssignedTo,
            hours: newHours
        }),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (msg) {
            // Notice that msg.d is used to retrieve the result object
            rePopulateUpdatedTaskControls(taskGuid, msg.d);
        },
        error: function (msg) {
            // Notice that msg.d is used to retrieve the result object
            alert(msg);
        }
    });
}

function addNewTask(storyGUID) {

    // Get Description
    var newDescription = document.getElementById("ModalTaskShortDescription").value;

    // Get Status
    var statusDropdown = document.getElementById("ModalTaskStatusDropdown");
    var newStatus = statusDropdown.options[statusDropdown.selectedIndex].value;

    // Get AssignedTo
    var assignedToDropdown = document.getElementById("ModalTaskAssignedToDropdown");
    var newAssignedTo = assignedToDropdown.options[assignedToDropdown.selectedIndex].value;

    // Get Hours
    var newHours = document.getElementById("ModalTaskHours").value;

    //string storyGUID, string shortDescription, string statusValue, string assignedToGUID
    $.ajax({
        type: 'POST',
        url: 'WorkList.aspx/AddTask',
        data: JSON.stringify({
            storyGUID: storyGUID,
            shortDescription: newDescription,
            statusValue: newStatus,
            assignedToGUID: newAssignedTo,
            hours: newHours
        }),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (msg) {
            // Notice that msg.d is used to retrieve the result object
            AddTaskToTable(storyGUID, msg.d);
        },
        error: function (msg) {
            // Notice that msg.d is used to retrieve the result object
            alert(msg);
        }
    });
}

function rePopulateUpdatedTaskControls(taskGUID, json) {

    // Parse the JSON object
    var returnValues = jQuery.parseJSON(json);

    // Set the updated values
    document.getElementById("taskShortDescriptionLink" + taskGUID).innerHTML = returnValues.ShortDescription;
    //document.getElementById("taskStatusWrapper" + taskGUID).innerHTML = returnValues.StatusHtml;
    document.getElementById("taskAssignedToSpan" + taskGUID).innerHTML = returnValues.AssignedTo;
    document.getElementById("taskHoursSpan" + taskGUID).innerHTML = returnValues.Hours;

    // Replace the Status SPAN with the new Status SPAN
    var statusWrapper = document.getElementById("taskStatusWrapper" + taskGUID);
    var statusParent = statusWrapper.parentNode;
    statusParent.innerHTML = returnValues.StatusHtml;
}

function AddTaskToTable(storyGUID, json) {

    // Parse the JSON object
    var returnValues = jQuery.parseJSON(json);
    var storyRowID = returnValues.StoryRowID;
    var rowHtml = returnValues.RowHtml;

    // Find the Table
    var table = document.getElementById("StoryGridView");

    // Determine where to insert the row
    var indexToInsertRow = FindIndexToInsertNewRow(table, storyRowID);

    // Create and populate a new row
    if (indexToInsertRow > 0) {
        var newTableRow = table.insertRow(indexToInsertRow);
        FormatNewRow(newTableRow, rowHtml);
    }
}

function FormatNewRow(newTableRow, rowHtml) {
    // Hydrate the row HTML
    var tempTable = document.createElement('table');
    tempTable.innerHTML = rowHtml;
    var row = tempTable.childNodes[0].childNodes[0];  // <table><tbody><tr>....

    // Replace the ID of the row created from the existing table
    newTableRow.id = row.id;
    newTableRow.className = row.className;

    // Replace the contents of the row created from the existing table
    newTableRow.innerHTML = row.innerHTML;
}

function FindIndexToInsertNewRow(table, storyRowID) {

    // Find the LAST Task row for this Story  (all rows for this story will be grouped together)
    var foundStoryRows = false;
    var indexToInsertRow = -1;
    for (var i = 0, row; row = table.rows[i]; i++) {
        // If the Row ID starts with the StoryRowID, then it belongs to this story
        var isRowForStory = row.id.indexOf(storyRowID) == 0;

        // If we already found some rows for this Story, but this row isn't one of them, 
        //  then this is where we need to insert the new row
        if (foundStoryRows == true && isRowForStory == false) {
            indexToInsertRow = i;
            break;
        }

        // If we found a row for this story, flip is "found row" flag
        if (isRowForStory) {
            foundStoryRows = true;
        }
    }

    return indexToInsertRow;
}
