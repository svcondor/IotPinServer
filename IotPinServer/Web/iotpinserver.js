var PinServer = (function () {

    // jquery crib
    // $(this).      = current element
    // $('div').     = all div elements
    // $('.class1'). = all elements with class='class1'
    // $('#id1').    = element with id='id1'

    var pinList = [];    // Array for all Gpio pins
    var leftPos = 0;   // Display position of next available pin column
    var columnWidth = 40;    // Display width of a pin column

    function setupHandlers() {

        // onClick open checkbox - Tell server to Open or Close a pin
        $('.open').click(function () {
            var value1 = $(this).prop('checked');
            sendUpdateToServer(this, "open", value1);
        });

        // onClick High checkbox - Tell server to set Pin High or Low
        $('.high').click(function () {
            var value1 = $(this).prop('checked');
            sendUpdateToServer(this, "setvalue", value1);
        });

        // onClick Radio button - Tell server to set pin driveMode
        $('.radio').click(function () {
            var drvMode1 = $(this).attr('value');
            sendUpdateToServer(this, "setdrivemode", drvMode1);
        });

        $('#reconnect').click(function () {
            poll(1);
        });
    }

    function sendUpdateToServer(element, cmd, value) {
        var v1 = this;
        var pinNumber = $(element).parent().closest('div').attr("id");
        console.log(cmd, pinNumber, value);
        var parent = $(element).parent().closest('div');
        $(parent).addClass("disabledContent");
        $.get('api', { cmd: cmd, pin: pinNumber, value: value });
    }


    // Function to poll server for new/changed pin data
    // first call retrieves all pins, subsequent calls only updated pins
    function poll(firstTime) {
        $.ajax({
            url: "api?cmd=pinstatus",
            data: { firstTime: firstTime },
            dataType: "json",
            timeout: 15000,
            success: function (pinList1) {
                for (var ix1 = 0; ix1 < pinList1.length; ++ix1) {
                    displayPinData(pinList1[ix1]);
                }
                if (firstTime === "1") {
                    // Set up handlers for checkboxes and radio buttons after all pins are displayed
                    setupHandlers();
                }
                $("body").css("cursor", "default");
                //Setup the next poll recursively
                poll("0");
            },
            error: function (xhr, status, errorThrown) {
                console.log("Error: " + errorThrown);
                console.log("Status: " + status);
                $("body").css("cursor", "default");
                $("#pinBoard").empty();
                var button = '<button style="font-family: sans-serif; font-size: 25px;" id="reconnect" class="hidden" type="button">Server has disconnected. Reconnect?</button>';
                $("#pinBoard").append(button);
                $('#reconnect').click(function () {
                    location.reload(true);
                });
            }
        });
    }


    // display initial or updated data for a single pin
    function displayPinData(pin1) {

        var find1 = $.grep(pinList, function (e) { return e.pinNumber === pin1.pinNumber; });
        if (find1.length === 0) {
            pinList.push(pin1);
            addNewDisplayColumn(pin1.pinNumber);
        }

        var pinDiv = $('#' + pin1.pinNumber).get(0);
        if (pin1.open === 2) {
            $(pinDiv).addClass("disabledContent");
        }
        else {
            var openCheckbox = $(pinDiv).find('.open')[0];
            if (pin1.open) $(openCheckbox).prop('checked', true);
            else $(openCheckbox).prop('checked', false);

            var highCheckbox = $(pinDiv).find('.high')[0];
            if (pin1.value) $(highCheckbox).prop('checked', true);
            else $(highCheckbox).prop('checked', false);

            //var allRadio = $(pinDiv).find('.radio');
            //$('#' + pin1.pinNumber + ' input:radio').val([pin1.driveMode]);

            $("input[name=r" + pin1.pinNumber + "]").val([pin1.driveMode]);
            $(pinDiv).removeClass("disabledContent");
        }
    }


    // Add a display column for a single pin
    function addNewDisplayColumn(pinNumber) {

        // HTML data to create a single pin column
        // %1 is replaced by pin number
        // %2 is replaced by left position of column
        var col1 = '<div id="%1" style="left:%2px">%1<br/>';
        col1 += '<input type="checkbox" class="open"><br/>';
        col1 += '<input type="checkbox" class="high"><br/><br/>';
        col1 += '<input type="radio" name="r%1" class="radio" value="0"><br/>';
        col1 += '<input type="radio" name="r%1" class="radio" value="2"><br/>';
        col1 += '<input type="radio" name="r%1" class="radio" value="3"><br/><br/>';
        col1 += '<input type="radio" name="r%1" class="radio" value="1"><br/>';
        col1 += '<input type="radio" name="r%1" class="radio" value="4"><br/>';
        col1 += '<input type="radio" name="r%1" class="radio" value="5"><br/>';
        col1 += '<input type="radio" name="r%1" class="radio" value="6"><br/>';
        col1 += '<input type="radio" name="r%1" class="radio" value="7"><br/>';
        col1 += '</div>';

        String.prototype.replaceAll1 = function (search, replace) {
            if (replace === undefined) {
                return this.toString();
            }
            return this.split(search).join(replace);
        };

        var col2 = col1.replaceAll1('%1', pinNumber);
        var col3 = col2.replaceAll1('%2', leftPos);
        $("#pinBoard").append(col3);
        leftPos += columnWidth;
    }


    $(document).ready(function () {
        $("body").css("cursor", "progress");
        //debugger;
        var labelsWidth = $('#labels').width();
        $('#pinBoard').css({ left: labelsWidth + 40 });
        // get machine type and machine name to page
        $.get("api?cmd=machinedata", function (data) {
            $('h2').text("IotPinServer - " + data[1] + ' - ' + data[0]);
        });

        // Start a poll of the server for GPIO pin data
        poll("1");
    });

})();