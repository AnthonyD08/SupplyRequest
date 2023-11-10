$(document).ready(function () {
    var codigoRequest;

    $('#RequestTabla').DataTable({
        dom: 'Bfrtip',
        buttons: [
            'copyHtml5',
            'excelHtml5',
            'csvHtml5',
            'pdfHtml5'
        ]
    });

    $("#btnMensajeExitoso").click(function () {
        $('#modalVentanaExitosa').modal('hide');
        limpiarCamposRequest();
        location.reload();
    });

    $("#btnMensajeFallido").click(function () {
        $('#modalVentanaFallida').modal('hide');
        location.reload();
    });

    $("#btnAgregarRequest").click(function () {
        if (validarCamposRequest()) {

            var codigoRequest = document.getElementById("IDAgregarRequestRequestID").value;

            if (codigoRequest == "") {

                $.ajax({
                    url: '/Request/',
                    type: 'POST',
                    data: JSON.stringify({
                            RequestId: 0, //Se asigna a posteriori
                            Employee: 0, // Se obtiene del back-end
                            Accountant: 0, // Se asigna a posteriori
                            ProductName: document.getElementById("IDAgregarRequestProductName").value,
                            Price: document.getElementById("IDAgregarRequestPrice").value,
                            Quantity: document.getElementById("IDAgregarRequestQuantity").value,
                            Active: true
                        }
                    ),
                    async: true,
                    cache: false,
                    contentType: 'application/json',
                    success: function (result) {
                        limpiarCamposRequest();
                        $('#modalVentanaExitosa').modal('show');
                    },
                    error: function (request, status, err) {
                    }
                });
            }
            else {
                let accountantValue;

                if (document.getElementById("IDAgregarRequestAccountantID").value === "") {
                    accountantValue = null;
                }
                else {
                    accountantValue = document.getElementById("IDAgregarRequestAccountantID").value;
                }

                $.ajax({
                    url: '/Request/',
                    type: 'PUT',
                    data: JSON.stringify({
                            RequestId: codigoRequest,
                            Employee: document.getElementById("IDAgregarRequestEmployeeID").value,
                            Accountant: accountantValue,
                            ProductName: document.getElementById("IDAgregarRequestProductName").value,
                            Price: document.getElementById("IDAgregarRequestPrice").value,
                            Quantity: document.getElementById("IDAgregarRequestQuantity").value,
                            Active: document.getElementById("IDAgregarRequestActive").checked
                        }
                    ),
                    async: true,
                    cache: false,
                    contentType: 'application/json',
                    success: function (result) {
                        limpiarCamposRequest();
                        $('#modalVentanaExitosa').modal('show');
                    },
                    error: function (request, status, err) {
                        // En buena teoría este solo devuelve OK o HTTP 422 (Unprocessable Entity)
                        $('#modalVentanaFallida').find('.modal-body p').text(request.responseText);

                        $('#modalAgregarRequest').modal('hide');
                        $('#modalVentanaFallida').modal('show');
                    }
                });
            }

        }
    });

    $("#btnCancelarRequest").click(function () {
        limpiarCamposRequest();
    });


    $("a[name='btnEditarRequest']").click(function () {
        $("#modalAgregarRequest").find('h4').text('Editar Request');
        $("#btnAprobarRequest").show();
        $("#btnRechazarRequest").show();

        codigoRequest = $(this).data("id");
        VerDetalleRequest(codigoRequest);
    });

    $('#btnAprobarRequest').click(function () {
        var codigoRequest = document.getElementById("IDAgregarRequestRequestID").value;

        $.ajax({
            url: '/Request/Approve?requestId=' + codigoRequest,
            type: 'POST',
            async: true,
            cache: false,
            success: function (result) {
                limpiarCamposRequest();
                $('#modalAgregarRequest').modal('hide');
                $('#modalVentanaExitosa').modal('show');
            },
            error: function (request, status, err) {
                // HTTP 422 (Unprocessable Entity)
                $('#modalVentanaFallida').find('.modal-body p').text(request.responseText);

                $('#modalAgregarRequest').modal('hide');
                $('#modalVentanaFallida').modal('show');
            }
        });
    });

    $('#btnRechazarRequest').click(function () {
        var codigoRequest = document.getElementById("IDAgregarRequestRequestID").value;

        $('#modalAgregarRequest').modal('hide');

        $('#modalVentanaEliminarRequest').modal('show');
    });

    $("a[name='btnEliminarRequest']").click(function () {

        codigoRequest = $(this).data("id");
        $('#modalVentanaEliminarRequest').modal('show');
    });

    $("#btnAbrirDialogAgregarRequest").click(function () {
        limpiarCamposRequest();

        $("#modalAgregarRequest").find('h4').text('Agregar Request');
        $("#btnAprobarRequest").hide();
        $("#btnRechazarRequest").hide();

        $.ajax({
            url: '/User/CurrentUser',
            type: 'GET',
            async: true,
            cache: false,
            success: function (result) {
                let user = JSON.parse(result);
                document.getElementById("IDAgregarRequestEmployeeID").value = user.UserId;
                document.getElementById("IDAgregarRequestActive").checked = true;

                $('#modalAgregarRequest').modal('show');
            },
            error: function (request, status, err) {
            }
        });

        document.getElementById("IDAgregarRequestEmployeeID").value = "";
    });

    $("#btnAceptarEliminarRequestConfirmacion").click(function () {
        $.ajax({
            url: '/Request?requestId=' + codigoRequest,
            type: 'DELETE',
            async: true,
            cache: false,
            //contentType: 'application/json',
            success: function (result) {
                limpiarCamposRequest();
                $('#modalVentanaEliminarRequest').modal('hide');
                $('#modalVentanaExitosa').modal('show');
            },
            error: function (request, status, err) {
                $('#modalVentanaEliminarRequest').modal('hide');

                // HTTP 422 (Unprocessable Entity)
                if (request.status == 422) {

                    $('#modalVentanaFallida').find('.modal-body p').text('El registro ya está inactivo');
                    $('#modalVentanaFallida').modal('show');
                }
            }
        });
    });

    $("#btnCancelarEliminarRequestConfirmacion").click(function () {
        $('#modalVentanaEliminarRequest').modal('hide');
        location.reload();
    });

    $("#textoBuscarRequest").on("keyup", function () {
        var value = $(this).val().toLowerCase();

        var value = $(this).val().toLowerCase();
        $("#RequestTabla tr").filter(function () {
            $(this).toggle($(this).find('td:eq(0), td:eq(1), td:eq(2)').text().replace(/\s+/g, ' ').toLowerCase().indexOf(value) > -1)
        });
    });
});



function limpiarCamposRequest() {
    $('#modalAgregarRequest').modal('hide');

    document.getElementById("IDAgregarRequestRequestID").value = "";
    document.getElementById("IDAgregarRequestAccountantID").value = "";
    document.getElementById("IDAgregarRequestProductName").value = "";
    document.getElementById("IDAgregarRequestPrice").value = "";
    document.getElementById("IDAgregarRequestQuantity").value = "";
    document.getElementById("IDAgregarRequestActive").checked = false;

    $("#IDAgregarRequestRequestID").css('border', '1px solid #ced4da');
    $("#IDAgregarRequestAccountantID").css('border', '1px solid #ced4da');
    $("#IDAgregarRequestProductName").css('border', '1px solid #ced4da');
    $("#IDAgregarRequestPrice").css('border', '1px solid #ced4da');
    $("#IDAgregarRequestQuantity").css('border', '1px solid #ced4da');
}

function validarCamposRequest() {
    var bandera = true;
    var agregarRequestProductName = document.getElementById("IDAgregarRequestProductName").value;
    var agregarRequestPrice = document.getElementById("IDAgregarRequestPrice").value;
    var agregarRequestQuantity = document.getElementById("IDAgregarRequestQuantity").value;

    if (agregarRequestProductName == "") {
        $("#IDAgregarRequestProductName").css("border", "1px solid red");
        bandera = false;
    } else {
        $('#IDAgregarRequestProductName').css('border', '1px solid #ced4da');
    }
    if (agregarRequestPrice == "") {
        $("#IDAgregarRequestPrice").css("border", "1px solid red");
        bandera = false;
    } else {
        $('#IDAgregarRequestPrice').css('border', '1px solid #ced4da');
    }
    if (agregarRequestQuantity == "") {
        $("#IDAgregarRequestQuantity").css("border", "1px solid red");
        bandera = false;
    } else {
        $('#IDAgregarRequestQuantity').css('border', '1px solid #ced4da');
    }

    return bandera;

}

function VerDetalleRequest(codigo) {

    $.ajax({
        url: '/Request?requestId=' + codigo,
        type: 'GET',
        async: true,
        cache: false,
        // contentType: 'application/json',
        success: function (response) {
            document.getElementById("IDAgregarRequestRequestID").value = response.requestId;
            document.getElementById("IDAgregarRequestEmployeeID").value = response.employee;
            document.getElementById("IDAgregarRequestAccountantID").value = response.accountant;
            document.getElementById("IDAgregarRequestProductName").value = response.productName;
            document.getElementById("IDAgregarRequestQuantity").value = response.quantity;
            document.getElementById("IDAgregarRequestPrice").value = response.price;
            document.getElementById("IDAgregarRequestActive").checked = response.active;
        },
        error: function (request, status, err) {
        }
    });

    $('#modalAgregarRequest').modal('show');
}

function filterInputNum(event) {
    const inputField = event.target;
    const inputValue = inputField.value;

    // Si el valor ingresado no es un número positivo, establece el valor del campo en vacío
    if (isNaN(inputValue) || Number(inputValue) <= 0) {
        inputField.value = '';
    }
}