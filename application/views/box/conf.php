<?php $this->load->view('common/header'); ?>
<script type="text/javascript" src="http://maps.google.com/maps/api/js?sensor=true"></script>
<script type="text/javascript" src="<?php echo base_url(); ?>html/js/gmaps.js"></script>
<script type="text/javascript">
    var map;
    $(document).ready(function() {
        map = new GMaps({
            div: '#map_canvas',
            lat: $("#y").text(),
            lng: $("#x").text(),
            zoom: 16
        });
        map.addMarker({
            lat: $("#y").text(),
            lng: $("#x").text(),
            title: $("#box_id").text()
        });
    });

</script>
<style>
    #map_canvas {
        width: 100%;
        height:500px;
    }
</style>
<div class="container-fluid">
    <ul class="breadcrumb">
        <li>端末登録<span class="divider"></span></li>
        <li class="active">端末登録確認</li>
    </ul>
    <h2>端末登録確認</h2>
    <?php echo validation_errors(); ?>
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">端末情報</div>
        </div>
        <div class="panel-body">
            <table class="table table-bordered">
                <tr>
                    <th>経度</th>
                    <td id="x"><?php echo $map['x']; ?></td>
                </tr>
                <tr>
                    <th>緯度</th>
                    <td id="y"><?php echo $map['y']; ?></td>
                </tr>
                <tr>
                    <th>設置住所</th>
                    <td><?php echo $address; ?></td>
                </tr>
                <tr>
                    <th>マップ</th>
                    <td><div id="map_canvas"></div></td>
                </tr>
            </table>
        </div>
        <?php
        $url = 'box/register/';
        echo form_open($url);
        ?>
        <?php
        echo form_hidden('address', $address);
        ?>
        <div class="text-center" style="margin-bottom:10px;"><button type="submit" class="btn btn-success btn-lg">登録</button></div>
    </div>
</div>
<script type="text/javascript" src="https://www.google.com/jsapi"></script>

<?php $this->load->view('common/footer'); ?>
