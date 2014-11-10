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
</div>
<?php echo $this->load->view('common/footer') ?>