function showModalIncidentPopup(incidentGUID) {

    var popupTitle = "Update Incident";

    // Populate the controls on the Modal popup
    populateModalIncidentControls(incidentGUID);

    // Display the modal popup
    $("#ModalIncidentUpdateDialog").dialog({
        title: popupTitle,
        dialogClass: "projectGroup",
        width: 700,
        modal: true,
        buttons: {
            Update: function () {

                updateIncidentValues(incidentGUID);

                $(this).dialog('close');
            },
            Cancel: function () {
                $(this).dialog('close');
            }
        }
    });
}

function populateModalIncidentControls(incidentGUID) {

    // Get the current values
    var incidentStatus = document.getElementById("incidentStatusLink" + incidentGUID).innerText;
    var incidentAssignedTo = document.getElementById("incidentAssignedToSpan" + incidentGUID).innerText;
    var incidentDescription = document.getElementById("incidentShortDescriptionLink" + incidentGUID).innerText;

    // Get the target objects
    var modalIncidentShortDescription = document.getElementById("ModalIncidentShortDescription");
    var modalIncidentStatus = document.getElementById("ModalIncidentStatusDropdown");
    var modalIncidentAssignedTo = document.getElementById("ModalIncidentAssignedToDropdown");

    // Short Description
    modalIncidentShortDescription.value = incidentDescription;

    // Status
    for (var index = 0; index < modalIncidentStatus.options.length; index++) {
        if (modalIncidentStatus.options[index].innerHTML == incidentStatus) {
            modalIncidentStatus.selectedIndex = index;
            break;
        }
    }

    // Assigned To
    for (var index = 0; index < modalIncidentAssignedTo.options.length; index++) {
        if (modalIncidentAssignedTo.options[index].innerHTML == incidentAssignedTo) {
            modalIncidentAssignedTo.selectedIndex = index;
            break;
        }
    }
}

function updateIncidentValues(incidentGuid) {

    // Get Description
    var newDescription = document.getElementById("ModalIncidentShortDescription").value;

    // Get Status
    var statusDropdown = document.getElementById("ModalIncidentStatusDropdown");
    var newStatus = statusDropdown.options[statusDropdown.selectedIndex].value;

    // Get AssignedTo
    var assignedToDropdown = document.getElementById("ModalIncidentAssignedToDropdown");
    var newAssignedTo = assignedToDropdown.options[assignedToDropdown.selectedIndex].value;

    //string taskGUID, string shortDescription, string statusValue, string assignedToGUID
    $.ajax({
        type: 'POST',
        url: 'WorkList.aspx/UpdateIncident',
        data: JSON.stringify({
            incidentGUID: incidentGuid,
            shortDescription: newDescription,
            statusValue: newStatus,
            assignedToGUID: newAssignedTo
        }),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (msg) {
            // Notice that msg.d is used to retrieve the result object
            rePopulateUpdatedIncidentControls(incidentGuid, msg.d);
        },
        error: function (msg) {
            // Notice that msg.d is used to retrieve the result object
            alert(msg);
        }
    });
}

function rePopulateUpdatedIncidentControls(incidentGUID, json) {

    // Parse the JSON object
    var returnValues = jQuery.parseJSON(json);

    // Set the updated values
    document.getElementById("incidentShortDescriptionLink" + incidentGUID).innerHTML = returnValues.ShortDescription;
    //document.getElementById("incidentStatusWrapper" + incidentGUID).innerHTML = returnValues.StatusHtml;
    document.getElementById("incidentAssignedToSpan" + incidentGUID).innerHTML = returnValues.AssignedTo;

    // Replace the Status SPAN with the new Status SPAN
    var statusWrapper = document.getElementById("incidentStatusWrapper" + incidentGUID);
    var statusParent = statusWrapper.parentNode;
    statusParent.innerHTML = returnValues.StatusHtml;
}


