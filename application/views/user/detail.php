<?php echo $this->load->view('common/header') ?>
<div class="panel panel-info">
    <div class="panel-heading">
        <div class="panel-title">ユーザ情報</div>
    </div>
    <div class="panel-body">
        <table class="table table-bordered">
            <?php
            foreach ($user as $val) {
                ?>
                <tr>
                    <th>ニックネーム</th>
                    <td><?php echo $val->nickname ?></td>
                </tr>
                <tr>
                    <th>登録日</th>
                    <td><?php echo $val->entry_date ?></td>
                </tr>
                <tr>
                    <th>更新日</th>
                    <td><?php echo $val->update_date ?></td>
                </tr>
                <rt>
                <th>生年月日</th>
                <td><?php echo $val->birthday ?></td>
                </tr>
                <?php
            }
            ?>
        </table>
    </div>
    <table class="table table-hover">
        <tr class="success">
            <th class="col-xs-4 col-sm-4 col-md-4">端末ID</th>
            <th class="col-xs-4 col-sm-4 col-md-4">曲ID<th>
            <th class="col-xs-4 col-sm-4 col-md-4">利用日</th>
        </tr>
        <?php if (!empty($list)) { ?>
            <?php foreach ($list as $val) { ?>
                <tr>
                    <td><?php echo $val->box_id ?></td>
                    <td><?php echo $val->song_id ?></td>
                    <td><?php echo $val->use_datetime ?></td>
                </tr>
                <?php
            }
        }
        ?>
    </table>
</div>
<?php echo $this->load->view('common/footer') ?>