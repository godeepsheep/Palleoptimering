$(document).ready(function () {
    $("#save-elements-btn").on("click", function () {
        const data = {};
        $("tbody tr").each(function () {
            const instanceId = $(this).find("select").attr("name").match(/elements\[(.*?)\]/)[1];
            const rotationRules = $(this).find("select").val();
            const isSpecial = $(this).find("input[type=checkbox]").is(":checked");
            const maxElementsPerPallet = $(this).find("input[type=number]").val();

            data[instanceId] = {
                InstanceId: instanceId,
                RotationRules: parseInt(rotationRules),
                IsSpecial: isSpecial,
                MaxElementsPerPallet: maxElementsPerPallet ? parseInt(maxElementsPerPallet) : null
            };
        });

        $.ajax({
            url: "/Elements/SaveAllElements",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            headers: {
                "RequestVerificationToken": $("input[name='__RequestVerificationToken']").val()
            },
            success: function (response) {
                alert("Elements saved successfully!");
            },
            error: function (xhr, status, error) {
                alert("An error occurred while saving: " + xhr.responseText);
            }
        });
    });
});
