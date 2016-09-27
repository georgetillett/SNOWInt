function showModalStoryPopup(storyGUID) {

    // Populate the controls on the Modal popup
    populateModalStoryControls(storyGUID);

    // Display the modal popup
    $("#ModalStoryUpdateDialog").dialog({
        title: "Update Story",
        dialogClass: "projectGroup",
        width: 700,
        modal: true,
        buttons: {
            Update: function () {

                updateStoryValues(storyGUID);

                $(this).dialog('close');
            },
            Cancel: function () {
                $(this).dialog('close');
            }
        }
    });
}

function populateModalStoryControls(storyGUID) {

    // Get the current values
    var storyStatus = document.getElementById("storyStatusLink" + storyGUID).innerText;
    var storyAssignedTo = document.getElementById("storyAssignedToSpan" + storyGUID).innerText;
    var storyDescription = document.getElementById("storyShortDescriptionLink" + storyGUID).innerText;
    var storyBlocked = document.getElementById("storyBlockedSpan" + storyGUID).innerText;
    var storyBlockedReason = document.getElementById("storyBlockedReasonSpan" + storyGUID).innerText;
    var storyExpenseType = document.getElementById("storyExpenseTypeSpan" + storyGUID).innerText;

    // Get the target objects
    var modalShortDescription = document.getElementById("ModalStoryShortDescription");
    var modalStoryStatus = document.getElementById("ModalStoryStatusDropdown");
    var modalStoryAssignedTo = document.getElementById("ModalStoryAssignedToDropdown");
    var modalBlocked = document.getElementById("ModalStoryBlockedCheckbox");
    var modalBlockedReason = document.getElementById("ModalStoryBlockedReason");
    var modalStoryExpenseTypeOptionOpEx = document.getElementById("ModalStoryExpenseTypeOptionOpEx");
    var modalStoryExpenseTypeOptionCapEx = document.getElementById("ModalStoryExpenseTypeOptionCapEx");
    var jmodalStoryExpenseTypeOptionOpEx = $("#ModalStoryExpenseTypeOptionOpEx");
    var jmodalStoryExpenseTypeOptionCapEx = $("#ModalStoryExpenseTypeOptionCapEx");
    

    // Short Description
    modalShortDescription.value = storyDescription;

    // Status
    for (var index = 0; index < modalStoryStatus.options.length; index++) {
        if (modalStoryStatus.options[index].innerHTML == storyStatus) {
            modalStoryStatus.selectedIndex = index;
            break;
        }
    }

    // Assigned To
    for (var index = 0; index < modalStoryAssignedTo.options.length; index++) {
        if (modalStoryAssignedTo.options[index].innerHTML == storyAssignedTo) {
            modalStoryAssignedTo.selectedIndex = index;
            break;
        }
    }

    // Expense Type
    if (storyExpenseType == "OpEx") {
        jmodalStoryExpenseTypeOptionOpEx.attr('checked', 'checked');
        //jmodalStoryExpenseTypeOptionGroup.prop("checked", true); ;
    }
    else {
        jmodalStoryExpenseTypeOptionCapEx.attr('checked', 'checked');
        //jmodalStoryExpenseTypeOptionCapEx.prop("checked", true); ;
    }

    // Blocked and Blocked Reason
    if (storyBlocked == "BLOCKED") {
        modalBlocked.checked = true;
        modalBlockedReason.value = storyBlockedReason;
        modalBlockedReason.disabled = false;
    }
    else {
        modalBlocked.checked = false;
        modalBlockedReason.value = "";
        modalBlockedReason.disabled = true;
    }

}

function updateStoryValues(storyGuid) {

    // Get Description
    var newDescription = document.getElementById("ModalStoryShortDescription").value;

    // Get Status
    var statusDropdown = document.getElementById("ModalStoryStatusDropdown");
    var newStatus = statusDropdown.options[statusDropdown.selectedIndex].value;

    // Get AssignedTo
    var assignedToDropdown = document.getElementById("ModalStoryAssignedToDropdown");
    var newAssignedTo = assignedToDropdown.options[assignedToDropdown.selectedIndex].value;

    // Get ExpenseType
    var expenseTypeOptionOpEx = $("#ModalStoryExpenseTypeOptionOpEx");
    var expenseTypeOptionCapEx = $("#ModalStoryExpenseTypeOptionCapEx");
    var newExpenseType = expenseTypeOptionOpEx.is(':checked') ? expenseTypeOptionOpEx.val() : expenseTypeOptionCapEx.val();

    // Get Blocked
    var newBlocked = "false";
    if (document.getElementById("ModalStoryBlockedCheckbox").checked) {
        newBlocked = "true";
    }

    // Get Blocked Reason
    var newBlockedReason = document.getElementById("ModalStoryBlockedReason").value;


    //string storyGUID, string shortDescription, string statusValue, string assignedToGUID, string blocked, string blockedReason
    $.ajax({
        type: 'POST',
        url: 'WorkList.aspx/UpdateStory',
        data: JSON.stringify({ 
            storyGUID: storyGuid,
            shortDescription: newDescription,
            statusValue: newStatus,
            assignedToGUID: newAssignedTo,
            expenseType: newExpenseType,
            blocked: newBlocked,
            blockedReason: newBlockedReason}),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (msg) {
            // Notice that msg.d is used to retrieve the result object
            rePopulateUpdatedStoryControls(storyGuid, msg.d);
        },
        error: function (msg) {
            // Notice that msg.d is used to retrieve the result object
            alert(msg);
        }
    });
}

function rePopulateUpdatedStoryControls(storyGUID, json) {

    // Parse the JSON object
    var returnValues = jQuery.parseJSON(json);

    // Set the updated values
    document.getElementById("storyShortDescriptionLink" + storyGUID).innerHTML = returnValues.ShortDescription;
    document.getElementById("storyStatusWrapper" + storyGUID).innerHTML = returnValues.StatusHtml;
    document.getElementById("storyAssignedToSpan" + storyGUID).innerHTML = returnValues.AssignedTo;
    document.getElementById("storyExpenseTypeSpan" + storyGUID).innerHTML = returnValues.ExpenseType;
    document.getElementById("storyBlockedSpan" + storyGUID).innerHTML = returnValues.Blocked;
    document.getElementById("storyBlockedSpan" + storyGUID).title = returnValues.BlockedReason;
    document.getElementById("storyBlockedReasonSpan" + storyGUID).innerHTML = returnValues.BlockedReason;

    // Set the Row class (if needed)
    modalStorySetRowClass("storyExpenseTypeSpan" + storyGUID, returnValues.ExpenseType);
}

function toggleStoryBlockedReason(checkbox) {

    // If the story is Blocked, then enable the Blocked Reason textbox
    var textbox = document.getElementById("ModalStoryBlockedReason");
    textbox.disabled = !checkbox.checked;
}

function modalStorySetRowClass(controlName, expenseType) {
    var disableRow = false;

    // Determine whether the new ExpenseType value causes the item to be in the wrong Group list
    //  Values: 1=CapEx   2=OpEx
    if (expenseType == "CapEx" && document.getElementById("ProjectGroupOpEx").className == "projectGroupSelected") {
        disableRow = true;
    }
    if (expenseType == "OpEx" && document.getElementById("ProjectGroupCapEx").className == "projectGroupSelected") {
        disableRow = true;
    }


    // If the row should be disabled, then hide it
    if (disableRow) {

        // Find the ROW that contains this workitem
        var rowId = $("#" + controlName).parent().parent().get(0).id;
        var rowElement = $("#" + rowId);
        rowElement.addClass("hiddenControl");
    }
}