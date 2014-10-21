<?php $this->load->view('common/header'); ?>
<div class="container">
    <h2>ユーザ一覧</h2>
    <?php
    $attributes = array('role' => 'form');
    echo form_open('user/search', $attributes);
    ?>
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">検索</div>
        </div>
        <div class="panel-body">
            <lable>年齢&nbsp;</label><input name="age_over" type="text" placeholder="年齢(以上)" />&nbsp;～&nbsp;<input name="age_under" type="text" placeholder="年齢(以下)" />
            <lable>性別&nbsp;</label><input name="age_over" type="text" placeholder="年齢(以上)" />
            <lable>登録日&nbsp;</label><input name="age_over" type="text" placeholder="年齢(以上)" />&nbsp;～&nbsp;<input name="age_under" type="text" placeholder="年齢(以下)" />
            <lable>利用回数&nbsp;</label><input name="age_over" type="text" placeholder="年齢(以上)" />
        </div>
    </div>
</form>
<table class="table">
    <tr>
        <th>ニックネーム</th>
        <th>年齢</th>
        <th>性別</th>
        <th>登録日</th>
        <th>利用回数</th>
    </tr>
    <?php if (!empty($list)) { ?>
        <?php foreach ($list as $val) { ?>
            <tr>
                <td><?php echo $val->nickname ?></td>
                <td><?php echo $val->age ?></td>
                <td><?php echo $val->gender ?></td>
                <td><?php echo $val->entry_date ?></td>
                <td><?php echo $val->use_count ?></td>
            </tr>
            <?php
        }
    }
    ?>
</table>
</div>
<?php $this->load->view('common/footer'); ?>
