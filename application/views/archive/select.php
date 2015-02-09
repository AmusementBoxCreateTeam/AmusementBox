<?php echo $this->load->view('login/header') ?>
<div>
    <ul>
        <li>
            <a href="<?php echo base_url().'index.php/index' ?>">管理画面</a>
        </li>
        <li>
            <a href="<?php echo base_url().'index.php/square' ?>">Square DEMO</a>
        </li>   
        <li>
            <a href="<?php echo base_url().'index.php/archive/archive_site' ?>">アーカイブサイト</a>
        </li>
    </ul>
</div>
<?php
echo $this->load->view('login/footer');
