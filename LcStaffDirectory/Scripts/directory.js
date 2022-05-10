
$(document).ready(function () {

    //checkReferrer();  
    setupPagination();
    setDefaults();
});

function setDefaults() {
    // set rdo and div defaults
    $('.search-type').hide();

    var test = $('.rdo-btn:checked').val();
    $('#' + test).show();

    // if clicked
    $("[name='SearchType']").click(function () {
        if ($(this).is(":checked")) {
            $('.search-type').hide();
            $('#' + this.value).show('fast');
        }
        else {
            $('#' + this.value).hide('fast');
        }
    });
}

function setupPagination() {
    // Result Paging
    $(".page-first").click(function () {
        var disabled = $(this).find("input[type=image]").is(":disabled");
        if (!disabled) {
            $("#hf-pIndex").val(1);
            $("#search-btn").click();
        }
    });

    $(".page-last").click(function () {
        var disabled = $(this).find("input[type=image]").is(":disabled");
        if (!disabled) {
            var pcount = parseInt($("#hf-pCount").val(), 10);
            $("#hf-pIndex").val(pcount);
            $("#search-btn").click();
        }
    });

    $(".page-next").click(function () {
        var disabled = $(this).find("input[type=image]").is(":disabled");
        if (!disabled) {
            var pindex = parseInt($("#hf-pIndex").val(), 10);
            $("#hf-pIndex").val(pindex + 1);
            $("#search-btn").click();
        }
    });

    $(".page-prev").click(function () {
        var disabled = $(this).find("input[type=image]").is(":disabled");
        if (!disabled) {
            var pindex = parseInt($("#hf-pIndex").val(), 10);
            $("#hf-pIndex").val(pindex - 1);
            $("#search-btn").click();
        }
    });

    $('#pSize').on('change', function (e) {
        $("#hf-pIndex").val(1);
        $("#hf-pSize").val(this.value);
        $("#search-btn").click();
    });
}

function checkReferrer() {
    var validated = $("#hf-validated").val();
    if (validated === undefined || validated == false) {
        var ref = document.referrer;
        if (ref !== undefined && ref != null) {
            var params = { referrer: ref };
            post(index_path, params);
        }
    }
}

function post(path, params, fn_success, fn_error) {
    $.ajax({
        type: 'POST',
        url: path,
        data: params,
        success: function (response) {
            if (response == true) {
                location.reload();  
            }
            else {
                window.location.replace("http://www.lethbridgecollege.ca");
            }
            
        },
        error: function (response) {
            window.location.replace("http://www.lethbridgecollege.ca");
        }
    });
}
